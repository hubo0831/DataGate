using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 用户登录会话处理接口
    /// </summary>
    public interface ISessionProvider
    {
        Task<UserSession> Get(string token);

        Task<object> GetUserAsync(string token);

        Task<LoginResult> Login(LoginRequest request, bool validate);

        Task<bool> Remove(string token);
    }
}