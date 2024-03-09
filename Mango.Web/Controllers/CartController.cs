using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        public async Task<IActionResult> CartIndex()
        {
            return View(await CartInformationFromLoggedInUser());
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
    }
}
