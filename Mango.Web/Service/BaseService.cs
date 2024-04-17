
using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service
{
    public class BaseService : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenProvider _tokenProvider;
        public BaseService(IHttpClientFactory httpClientFactor, ITokenProvider tokenProvider)
        {
            _httpClientFactory = httpClientFactor;
            _tokenProvider = tokenProvider;
        }
        public async Task<ResponseDto?> SendAsync(RequestDto requestDto, bool withBearer = true)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient("MangoAPI");
                
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");
                //token


                message.RequestUri = new Uri(requestDto.Url);

				if (requestDto.ContentType == ContentType.MultipartFormData)
				{
                    var content =new MultipartFormDataContent();

                    foreach (var prop in requestDto.Data.GetType().GetProperties())
                    {
                        var value=prop.GetValue(requestDto.Data);
                        if(value is FormFile)
                        {
                            var file = (FormFile)value;
                            if (file != null)
                            {
                                content.Add(new StreamContent(file.OpenReadStream()),prop.Name,file.FileName);  
                            }
                        }
						else
						{
							content.Add(new StringContent(value == null ? "" : value.ToString()), prop.Name);
						}
					}
					message.Content = content;
				}
                else
                {
					if (requestDto.Data != null)
					{
						message.Content = new StringContent(JsonConvert.SerializeObject(requestDto.Data), Encoding.UTF8, "application/json");
					}
				}




				if (withBearer)
                {
                    var token = _tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                    
                }

                switch (requestDto.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                HttpResponseMessage? apiResponse = null;


                apiResponse = await client.SendAsync(message);
                

                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access Denied" };
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Access Denied" };
                    default:
                        var apiResponseContent = await apiResponse.Content.ReadAsStringAsync();
                        var apiResponseDto = JsonConvert.DeserializeObject<ResponseDto>(apiResponseContent);
                        return apiResponseDto;
                }
            }
            catch (Exception ex)
            {
                var dto = new ResponseDto() 
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
                return dto;
            }
        }
    }
}
