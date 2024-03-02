using Mango.Web.Models.Dto;

namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        Task<CouponDto?> GetCouponAsync(string couponCode);
        Task<CouponDto?> GetAllCouponsAsync();
        Task<CouponDto?> GetCouponByIdAsync(int couponId);
        Task<CouponDto?> AddCouponAsync(CouponDto coupondto);
        Task<CouponDto?> UpdateCouponAsync(CouponDto coupondto);
        Task<CouponDto?> DeleteCouponAsync(int couponId);

    }
}
