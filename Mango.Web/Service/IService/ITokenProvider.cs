namespace Mango.Web.Service.IService
{
    public interface ITokenProvider
    {
        string? GetToken();
        void SetToken(string token);
        void ClearToken();
    }
}
