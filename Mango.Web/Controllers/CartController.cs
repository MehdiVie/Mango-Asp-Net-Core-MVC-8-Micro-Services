using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using System.IdentityModel.Tokens.Jwt;
using Mango.Services.OrderAPI.Data;
using Mango.Web.Utility;
using Mango.Services.OrderAPI.Models;
using Mango.MessageBus;
using Azure;


namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly AppDbContext _db;


        public CartController(ICartService cartService, IOrderService orderService,AppDbContext db,
            IConfiguration configuration,IMessageBus messageBus)
        {
            _cartService = cartService;
            _orderService = orderService;
            _configuration = configuration;
            _messageBus = messageBus;
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await CartInformationFromLoggedInUser());
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            return View(await CartInformationFromLoggedInUser());
        }

        [Authorize]
        [ActionName("Checkout")]
        [HttpPost]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
            CartDto cart = await CartInformationFromLoggedInUser();
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;
            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Name = cartDto.CartHeader.Name;


            var response = await _orderService.CreateOrder(cart);
            OrderHeaderDto orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));

            if (response != null && response.IsSuccess)
            {
                //get stripe session and redirect to stripe to place order
                //
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";

                StripeRequestDto stripeRequestDto = new()
                {
                    ApprovedUrl = domain + "cart/Confirmation?orderId=" + orderHeaderDto.OrderHeaderId,
                    CancelUrl = domain + "cart/checkout",
                    OrderHeader = orderHeaderDto
                };

                //var stripeResponse = CreateStripeSession(stripeRequestDto);
                //var stripeResponse = await _orderService.CreateStripeSession(stripeRequestDto);
                //StripeRequestDto stripeResponseResult = JsonConvert.DeserializeObject<StripeRequestDto>
                // (Convert.ToString(stripeResponse.Result));

                var options = new SessionCreateOptions()
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",

                };

                var DiscountsObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions()
                    {
                        Coupon = stripeRequestDto.OrderHeader.CouponCode
                    }
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

                if (stripeRequestDto.OrderHeader.Discount > 0)
                {
                    options.Discounts = DiscountsObj;
                }

                var service = new SessionService();
                Session session = service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;

                OrderHeader orderHeader = _db.OrderHeaders.Find(stripeRequestDto.OrderHeader.OrderHeaderId);

                orderHeader.StripeSessionId = session.Id;
                _db.SaveChanges();

                Response.Headers.Add("Location", stripeRequestDto.StripeSessionUrl);
                return new StatusCodeResult(303);

            }
            return View();
        }
        [Authorize]
        [ActionName("ValidateStripeSession")]
        private async Task<string> ValidateStripeSessionAsync(int orderHeaderId)
        {
            
            OrderHeader orderHeader = _db.OrderHeaders.First(u=>u.OrderHeaderId==orderHeaderId);
            
            var service = new SessionService();
            Session session = service.Get(orderHeader.StripeSessionId);
            
            var paymentIntentService = new PaymentIntentService();
            PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);
                
            if (paymentIntent.Status == "succeeded")
            {
                orderHeader.PaymentIntentId= paymentIntent.Id;
                orderHeader.Status = SD.Status_Approved;
                _db.SaveChanges();

                RewardDto rewardDto = new()
                {
                    OrderId = orderHeaderId,
                    RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
                    UserId = orderHeader.UserId,
                };
                string topicName = _configuration.GetValue<string>("TopicAndQueueNames:OrderCreatedTopic");
                await _messageBus.PublishMessage(rewardDto,topicName);
            }

            return orderHeader.Status;

        }
        public async Task<IActionResult> Confirmation(int orderId)
        {
            
            Task<string> task1 = ValidateStripeSessionAsync(orderId);
            string orderHeaderStatus = task1.Result;

            if (orderHeaderStatus == SD.Status_Approved)
            {
                return View(orderId);
            }
            //in error case
            return View(0);

        }
        private async Task<CartDto> CartInformationFromLoggedInUser()
        {
            string userID = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault().Value;

            ResponseDto? response = await _cartService.GetCartByUserIdAsync(userID);

            if(response != null && response.IsSuccess)
            {
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
                return cartDto;
            }

            return new CartDto();

        }

        public async Task<IActionResult> RemoveDetails(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? response = await _cartService.RemoveItemDetailsAsync(cartDetailsId);
            if (response != null & response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            ResponseDto? response = await _cartService.ApplyCouponAsync(cartDto);
            
            if (response.IsSuccess && response != null) 
            {
                TempData["success"] = "Coupon applied successfully!";
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["error"]=response?.Message;
            }

            return RedirectToAction(nameof(CartIndex));

        }

        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
        {
            ResponseDto? response = await _cartService.RemoveCouponAsync(cartDto);

            if (response.IsSuccess && response != null)
            {
                TempData["success"] = "Coupon removed successfully!";
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return RedirectToAction(nameof(CartIndex));

        }

        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cartDto)
        {
            CartDto cart = await CartInformationFromLoggedInUser();
            cart.CartHeader.Email = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Email)?.FirstOrDefault()?.Value;

            ResponseDto? response = await _cartService.EmailCartAsync(cart);

            if (response.IsSuccess && response != null)
            {
                TempData["success"] = "Email will be processed and sent shortly!";
                return RedirectToAction(nameof(CartIndex));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return RedirectToAction(nameof(CartIndex));

        }


    }
}
