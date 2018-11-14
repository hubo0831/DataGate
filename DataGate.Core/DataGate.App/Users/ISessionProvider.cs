using System.Threading.Tasks;

namespace DataGate.App
{
    /// <summary>
    /// 用户登录会话处理接口
    /// </summary>
    public interface ISessionProvider
    {
        UserSession Get(string token);

        Task<UserInfoResult> GetUserAsync(string token);

        Task<LoginResult> Login(LoginRequest request);

        bool Remove(string token);
    }
}