using Mango.Web.Models;
using Mango.Web.Models.Dto;


namespace Mango.Web.Service.IService
{
    public interface ICouponService
    {
        Task<ResponseDto?> GetCouponAsync(string couponCode);
        Task<ResponseDto?> GetAllCouponsAsync();
        Task<ResponseDto?> GetCouponByIdAsync(int couponId);
        Task<ResponseDto?> AddCouponAsync(CouponDto coupondto);
        Task<ResponseDto?> UpdateCouponAsync(CouponDto coupondto);
        Task<ResponseDto?> DeleteCouponAsync(int couponId);

    }
}
