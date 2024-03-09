using Mango.Services.ShoppingCardAPI.Service.IService;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Newtonsoft.Json;

namespace Mango.Services.ShoppingCardAPI.Service
{
    public class CouponService : ICouponService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CouponService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<CouponDto> GetCoupon(string couponId)
        {
            var client = _httpClientFactory.CreateClient("Coupon");
            var response = await client.GetAsync($"api/coupon/GetByCode/{couponId}");
            var apiContent = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            //var resp1 = Convert.ToString(resp.Result);
            //var resp2= JsonConvert.DeserializeObject<IEnumerable<ProductDto>>(resp1);

            if (resp != null && resp.IsSuccess)
            {
                return JsonConvert.DeserializeObject<CouponDto>(Convert.ToString(resp.Result));
            }

            return new CouponDto();
        }
    }
}
