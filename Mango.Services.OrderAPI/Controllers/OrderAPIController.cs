using AutoMapper;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using Mango.MessageBus;
using Microsoft.EntityFrameworkCore;
using Mango.Services.OrderAPI.Service.IService;

namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : Controller
    {
        private readonly AppDbContext _db;
        private ResponseDto _response;
        private IMapper _mapper;
        private IProductService _productService;
        private IMessageBus _messageBus;
        private IConfiguration _configuration;

        public OrderAPIController(AppDbContext db, IMapper mapper, IProductService productService,
            IMessageBus messageBus,IConfiguration configuration)
        {
            _db = db;
            _response = new ResponseDto();
            _mapper = mapper;
            _productService = productService;
            _messageBus= messageBus;
            _configuration = configuration;

        }
        [Authorize]
        [HttpGet("GetOrders")]
        public ResponseDto? Get(string? userId = "")
        {
            try
            {
                IEnumerable<OrderHeader> objList;
                if (User.IsInRole(SD.Role_Admin))
                {
                    objList =  _db.OrderHeaders.Include(u => u.OrderDetails).OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                else
                {
                    objList = _db.OrderHeaders.Include(u => u.OrderDetails).Where(u => u.UserId == userId).OrderByDescending(u => u.OrderHeaderId).ToList();
                }
                _response.Result = _mapper.Map<IEnumerable<OrderHeaderDto>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }


        [Authorize]
        [HttpGet("GetOrder/{id:int}")]
        public ResponseDto? Get(int id)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.Include(u=>u.OrderDetails).First(u=>u.OrderHeaderId== id);    
                _response.Result= _mapper.Map<OrderHeaderDto>(orderHeader);
            }
            catch (Exception ex)
            {
                _response.Message = ex.Message;
                _response.IsSuccess = false;
            }
            return _response;
        }

        [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cartDto)
        {
            try
            {
                OrderHeaderDto orderHeaderDto= _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
                orderHeaderDto.OrderTime=DateTime.Now;
                orderHeaderDto.Status=SD.Status_Pending;
                orderHeaderDto.OrderDetails=_mapper.Map<IEnumerable<OrderDetailsDto>>(cartDto.CartDetails);

                OrderHeader orderCreated=  _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId= orderCreated.OrderHeaderId;

                _response.Result=orderHeaderDto;

            }
            catch (Exception ex)
            {
                _response.IsSuccess=false;
                _response.Message=ex.Message;   
            }
            return _response;
        }
        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {

            try
            {
                var options = new SessionCreateOptions()
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",

                };


                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.ProductPrice * 100), // $20.99 -> 2099
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.ProductName
                            }
                        },
                        Quantity = item.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;

                OrderHeader orderHeader = _db.OrderHeaders.Find(stripeRequestDto.OrderHeader.OrderHeaderId);

                orderHeader.StripeSessionId = session.Id;
                _db.SaveChanges();

                _response.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }

            return _response;
        }

        [Authorize]
        [HttpPost("UpdateOrderStatus/{orderId:int}")]
        public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody]string newStatus)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderId);
                if (orderHeader != null)
                {
                    if(newStatus == SD.Status_Cancelled)
                    {
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = orderHeader.PaymentIntentId,
                        };

                        var service = new RefundService();  
                        Refund refund = service.Create(options);

                    }
                    orderHeader.Status = newStatus;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
            }
            return _response;
        }
    }
}
