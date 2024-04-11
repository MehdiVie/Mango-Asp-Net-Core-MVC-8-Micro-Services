using Mango.Web.Models;
using Mango.Web.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mango.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        public ProductController (IProductService ProductService)
        {
            _productService = ProductService;
        }
        public async Task<IActionResult> ProductIndex()
        {
            List<ProductDto> list = new();

            ResponseDto response = await _productService.GetAllProductsAsync();

            if((response != null) && (response.IsSuccess))
            {
                list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(list);
        }

        public async Task<IActionResult> ProductCreate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProductCreate(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response= await  _productService.CreateProductAsync(model);

                if ((response != null) && (response.IsSuccess))
                {
                    TempData["success"] = "Product created successfully!";
                    return RedirectToAction(nameof(ProductIndex));
                }
                else
                {
                    TempData["error"] = response?.Message;
                }


            }

            return View(model);
        }
        public async Task<IActionResult> ProductEdit(int ProductId)
        {

                 ResponseDto? response = await _productService.GetProductByIdAsync(ProductId);

                if ((response != null) && (response.IsSuccess))
                {

                    ProductDto? model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                    return View(model);
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            
            return NotFound();
        }
        [HttpPost]
        public async Task<IActionResult> ProductEdit(ProductDto model)
        {
            if (ModelState.IsValid)
            {
                ResponseDto? response = await _productService.GetProductByIdAsync(model.ProductId);

                if ((response != null) && (response.IsSuccess))
                {

                    var task = await _productService.UpdateProductAsync(model);

                    if (task.IsSuccess)
                    {
                        TempData["success"] = "Product updated successfully!";
                    }
                    else
                    {
                        TempData["error"] = task?.Message;
                    }

                    return RedirectToAction(nameof(ProductIndex), "Product");
                }
                else
                {
                    TempData["error"] = response?.Message;
                }
            }
            return View(model);
        }

        public async Task<IActionResult> ProductDelete(int productId)
        {

            ResponseDto? response = await _productService.GetProductByIdAsync(productId);

            if ((response != null) && (response.IsSuccess))
            {

                ProductDto? model = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
                return View(model);
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> ProductDelete(ProductDto model)
        {

            ResponseDto? response = await _productService.GetProductByIdAsync(model.ProductId);

            if ((response != null) && (response.IsSuccess))
            {

                var task = await _productService.DeleteProductAsync(model.ProductId);

                if (task.IsSuccess)
                {
                    TempData["success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["error"] = task?.Message;
                }

                return RedirectToAction(nameof(ProductIndex), "Product");
            }
            else
            {
                TempData["error"] = response?.Message;
            }

            return View(model);
        }


    }

    
}
