using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService (AppDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> AssignRole(string email, string rollName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToUpper() == email.ToUpper());

            if (user != null)
            {
                if(!_roleManager.RoleExistsAsync(rollName).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(rollName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, rollName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginReequestDto loginReequestDto)
        {
            var user= _db.ApplicationUsers.FirstOrDefault(u=>u.UserName.ToLower()== loginReequestDto.Username.ToLower());
            bool isValid = await _userManager.CheckPasswordAsync(user, loginReequestDto.Password);

            

            if ((user is null) || (!isValid))
            {
                return new LoginResponseDto()  
                            {
                            User = null,
                            Token=""
                            };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user,roles);
            
           
            UserDto userDto = new() { 
                    ID=user.Id,
                    Email=user.Email,
                    Name=user.Name,
                    PhoneNumber=user.PhoneNumber
            };

            return new LoginResponseDto()
            {
                User = userDto,
                Token = token
            };

            
        }

        public async Task<string> Register(RegisterationReequestDto registerationReequestDto)
        {
            ApplicationUser user = new()
            {
                UserName= registerationReequestDto.Email,
                Email= registerationReequestDto.Email,
                NormalizedEmail= registerationReequestDto.Email.ToUpper(),
                Name= registerationReequestDto.Name,
                PhoneNumber= registerationReequestDto.PhoneNumber
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registerationReequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(x => x.UserName == registerationReequestDto.Email);

                    UserDto userDto = new()
                    {
                        ID=userToReturn.Id,
                        Name=userToReturn.Name,
                        Email=userToReturn.Email,
                        PhoneNumber=userToReturn.PhoneNumber
                    };

                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }

            }
            catch (Exception ex)
            {

            }
            return "Error Encountered!";

        }
    }
}
