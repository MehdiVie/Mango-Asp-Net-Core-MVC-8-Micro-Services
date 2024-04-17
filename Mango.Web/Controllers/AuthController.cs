using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }
        public IActionResult Login()
        {
            LoginReequestDto loginReequestDto = new();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginReequestDto obj)
        {

            if (ModelState.IsValid)
            {
                ResponseDto responseDto = await _authService.LoginAsync(obj);
                
                if ((responseDto != null) && (responseDto.IsSuccess))
                {
                     LoginResponseDto loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>
                       (Convert.ToString(responseDto.Result));

                    await SignInUser(loginResponseDto);

                    _tokenProvider.SetToken(loginResponseDto.Token);

                    TempData["success"] = "Logged In Successfully!";
                    return RedirectToAction(nameof(Index),"Home");
                }
                else
                {
                    TempData["error"] = responseDto.Message;
                    return View(obj);
                }

            }
            else
            {
                TempData["error"] = "Username or Password is incorrect!";
                return View(obj);
            }
        }

        private async Task SignInUser(LoginResponseDto model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(model.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                            jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                            jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                            jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role,
                jwtToken.Claims.FirstOrDefault(u => u.Type == "role").Value));

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal);

        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction(nameof(Index), "Home");
        }

        public IActionResult Register()
        {
            var rolesList = new List<SelectListItem>
            {
                new SelectListItem {Text=SD.RoleCustomer,Value=SD.RoleCustomer},
                //new SelectListItem {Text=SD.RoleAdmin,Value=SD.RoleAdmin}
            };

            ViewBag.RoleList = rolesList;   

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterationReequestDto obj)
        {
            
            if (ModelState.IsValid)
            {
                ResponseDto response = await _authService.RegisterAsync(obj);
                ResponseDto assignRole;

                
                if ((response != null) && (response.IsSuccess))
                {
                    if (string.IsNullOrEmpty(obj.Role))
                    {
                        obj.Role = SD.RoleCustomer;
                    }


                    assignRole = await _authService.AssignRoleAsync(obj);

                    if ((assignRole != null) && (assignRole.IsSuccess))
                    {
                        TempData["success"] = "You registered successfully!";
                        return RedirectToAction(nameof(Login));
                    }
                }
                else
                {
                    TempData["error"] = response?.Message;
                }

            }
            else
            {
                TempData["error"] = "Error encoutered!";
            }

            var rolesList = new List<SelectListItem>
            {
                new SelectListItem {Text=SD.RoleCustomer,Value=SD.RoleCustomer},
                new SelectListItem {Text=SD.RoleAdmin,Value=SD.RoleAdmin}
            };

            ViewBag.RoleList = rolesList;

            return View(obj);

        }




    }
}
