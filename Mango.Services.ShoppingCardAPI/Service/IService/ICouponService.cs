using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCardAPI.Service.IService
{
    public interface ICouponService
    {
        Task<CouponDto> GetCoupon(string couponId);
    }
}
