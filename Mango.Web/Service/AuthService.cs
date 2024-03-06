using Mango.Web.Models;
using Mango.Web.Service.IService;
using Mango.Web.Utility;

namespace Mango.Web.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;
        public AuthService(IBaseService baseService)
        {
            _baseService = baseService;
        }
        public async Task<ResponseDto?> AssignRoleAsync(RegisterationReequestDto registerationReequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = registerationReequestDto,
                Url = SD.AuthAPIBase + "/api/auth/assignRole"
            },withBearer:false);
        }

        public async Task<ResponseDto?> LoginAsync(LoginReequestDto loginReequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = loginReequestDto,
                Url = SD.AuthAPIBase + "/api/auth/login"
            }, withBearer: false);
        }

        public async Task<ResponseDto?> RegisterAsync(RegisterationReequestDto registerationReequestDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = registerationReequestDto,
                Url = SD.AuthAPIBase + "/api/auth/register"
            }, withBearer: false);
        }
    }
}
