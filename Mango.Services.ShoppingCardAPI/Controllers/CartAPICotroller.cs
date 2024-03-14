using AutoMapper;
using Mango.MessageBus;
using Mango.Services.ShoppingCardAPI.Data;
using Mango.Services.ShoppingCardAPI.Models;
using Mango.Services.ShoppingCardAPI.Models.Dto;
using Mango.Services.ShoppingCardAPI.Service.IService;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCardAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPICotroller : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private IProductService _productService;
        private ICouponService _couponService;
        private IMessageBus _messageBus;
        private IConfiguration _configuration;
        public CartAPICotroller(AppDbContext db, IMapper mapper,IProductService productService, ICouponService couponService,
            IMessageBus messageBus, IConfiguration configuration)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
            _productService = productService;
            _couponService = couponService;
            _messageBus = messageBus;
            _configuration = configuration;
        }

        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var carHeaderFromDb= await _db.CartHeader.AsNoTracking().FirstOrDefaultAsync(u=>
                                                                        u.UserId == cartDto.CartHeader.UserId);
                if (carHeaderFromDb == null)
                {
                    //create cartheader and card details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeader.Add(cartHeader);
                    await _db.SaveChangesAsync();

                    cartDto.CartDetails.First().CartHeaderId = cartHeader.CartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();

                }
                else
                {
                    //update cartdetails
                    //check card details for new products
                    var cartDetailsFromDb= await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(u=>
                                           u.ProductId==cartDto.CartDetails.First().ProductId && 
                                           u.CartHeaderId == carHeaderFromDb.CartHeaderId);

                    if (cartDetailsFromDb == null)
                    {
                        //create cart details
                        cartDto.CartDetails.First().CartHeaderId = carHeaderFromDb.CartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        //update count in cart details
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.Count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;

                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();

                    }

                }
                _response.Result = cartDto; 

            }
            catch (Exception ex)
            {
                _response.Message= ex.Message.ToString();
                _response.IsSuccess=false;
            }
            return _response;
        }

        [HttpPost("RemoveCartDetails")]
        public async Task<ResponseDto> RemoveCartDetails([FromBody]int cartDetailsId)
        {
            try
            {
                var cartDetailsToDelete = await _db.CartDetails.FirstOrDefaultAsync(u =>
                                                                        u.CartDetailsId == cartDetailsId);
                if (cartDetailsToDelete != null)
                {
                    
                    //create cartheader and card details
                    int totalCartItems = _db.CartDetails.Where(u => u.CartHeaderId== cartDetailsToDelete.CartHeaderId).Count();

                    _db.CartDetails.Remove(cartDetailsToDelete);

                    if (totalCartItems == 1)
                    {
                        var cartHeaderToDelete = await _db.CartHeader.FirstOrDefaultAsync(u => u.CartHeaderId ==
                                                                                          cartDetailsToDelete.CartHeaderId);
                        _db.CartHeader.Remove(cartHeaderToDelete);
                    }

                    await _db.SaveChangesAsync();

                    _response.Result = true;

                }
                else
                {
                    _response.Message = "This Item does not exist!";
                    _response.IsSuccess = false;
                }
                

            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto> GetCart(string userId)
        {
            try
            {
                
                CartDto cartDto = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_db.CartHeader.FirstOrDefault(u => u.UserId == userId)),
                };
                cartDto.CartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_db.CartDetails.Where(u =>
                           u.CartHeaderId == cartDto.CartHeader.CartHeaderId));

                IEnumerable<ProductDto> productDtos =await _productService.GetProducts();


                foreach (var item in cartDto.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(u=>u.ProductId == item.ProductId);
                    cartDto.CartHeader.CartTotal += (item.Product.Price * item.Count);
                }

                if (!string.IsNullOrEmpty(cartDto.CartHeader.CouponCode))
                {
                    CouponDto couponDto = await _couponService.GetCoupon(cartDto.CartHeader.CouponCode);
                    
                    if(couponDto!=null && cartDto.CartHeader.CartTotal >= couponDto.MinAmount) 
                    {
                        cartDto.CartHeader.Discount = couponDto.DiscountAmount;
                        cartDto.CartHeader.CartTotal -= cartDto.CartHeader.Discount;
                    }

                }

                _response.Result = cartDto;
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;

        }

        [HttpPost("EmailCartRequest")]
        public async Task<ResponseDto> EmailCartRequest([FromBody] CartDto cartDto)
        {
            try
            {
                await _messageBus.PublishMessage(cartDto, _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue"));
                _response.Result = cartDto;

            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<ResponseDto> ApplyCoupon([FromBody]CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeader.FirstOrDefaultAsync(u=>u.UserId == cartDto.CartHeader.UserId);

                cartHeaderFromDb.CouponCode= cartDto.CartHeader.CouponCode;

                _db.CartHeader.Update(cartHeaderFromDb);

                await _db.SaveChangesAsync();

                _response.Result= cartDto;
                
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<ResponseDto> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb = await _db.CartHeader.FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);

                cartHeaderFromDb.CouponCode = "";

                _db.CartHeader.Update(cartHeaderFromDb);

                await _db.SaveChangesAsync();

                _response.Result = cartDto;

            }
            catch (Exception ex)
            {
                _response.Message = ex.Message.ToString();
                _response.IsSuccess = false;
            }
            return _response;
        }


    }
}
