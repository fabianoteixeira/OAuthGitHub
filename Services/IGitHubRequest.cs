using GitHubOAuth.Models;
using Refit;

namespace GitHubOAuth.Services
{
    public interface IGitHubRequest
    {
        [Post("/login/oauth/access_token")]
        Task<string> GetAcessToken(AcessTokenRequest request);

        [Get("/user")]
        [Headers("User-Agent: Mozilla/5.0 (Windows NT 10; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0")]
        Task<GitHubUserResponse> GetUser([Header("Authorization")] string authorization);
    }
}
