

using IdentityModel;
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Mango.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        public HomeController(IProductService ProductService, ICartService cartService)
        {
            _productService = ProductService;
            _cartService = cartService;
        }


        public async Task<IActionResult> Index()
        {
            List<ProductDto> list = new();

            ResponseDto response = await _productService.GetAllProductsAsync();

            if ((response != null) && (response.IsSuccess))
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(list);
        }
        [Authorize]
        public async Task<IActionResult> ProductDetails(int productId)
        {
            ProductDto model = new();

            ResponseDto response = await _productService.GetProductByIdAsync(productId);

            if ((response != null) && (response.IsSuccess))
            {
                model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ActionName("ProductDetails")]
        public async Task<IActionResult> ProductDetails(ProductDto productDto)
        {

                CartDto cartDto = new()
                {
                    CartHeader = new CartHeaderDto
                    {
                        UserId = User.Claims.Where(u => u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value
                    },
                };
                
                CartDetailsDto cartDetailsDto = new()
                {
                    Count = productDto.Count,
                    ProductId = productDto.ProductId
                };

                List<CartDetailsDto> cartDetailsDtos = new() { cartDetailsDto };
                
                cartDto.CartDetails = cartDetailsDtos;

                ResponseDto? response = await _cartService.CartUpsertAsync(cartDto);

                if(response != null && response.IsSuccess)
                {
                    TempData["success"] = "Item has been added to cart!";
                //return RedirectToAction(nameof(Views_Cart_CartIndex),"Cart");
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["error"]=response?.Message;
                }

                return View(productDto);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
