using Mango.Web.Models;

namespace Mango.Web.Service.IService
{
    public interface IAuthService
    {
        Task<ResponseDto?> LoginAsync(LoginReequestDto loginReequestDto);
        Task<ResponseDto?> RegisterAsync(RegisterationReequestDto registerationReequestDto);
        Task<ResponseDto?> AssignRoleAsync(RegisterationReequestDto registerationReequestDto);

    }
}
