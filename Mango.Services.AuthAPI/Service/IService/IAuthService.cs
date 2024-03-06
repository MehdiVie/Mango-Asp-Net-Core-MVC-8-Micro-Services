using Mango.Services.AuthAPI.Models.Dto;

namespace Mango.Services.AuthAPI.Service.IService
{
    public interface IAuthService
    {
        Task<string> Register(RegisterationReequestDto registerationReequestDto);

        Task<LoginResponseDto> Login(LoginReequestDto loginReequestDto);

        Task<bool> AssignRole(string email, string rollName);
    }
}
