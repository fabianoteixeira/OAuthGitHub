using GitHubOAuth.Models;
using GitHubOAuth.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Refit;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime;
using System.Runtime.Intrinsics.Arm;
using System.Web;

namespace GitHubOAuth.Controllers
{
    public class HomeController : Controller
    {
        private HttpClient _httpClient { get; set; }
        private readonly ILogger<HomeController> _logger;
        private readonly IGitHubRequest _github;
        public HomeController(ILogger<HomeController> logger, IGitHubRequest github)
        {
            _logger = logger;
            _github = github;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://github.com/");
        }

        public IActionResult Index()
        {
            return View();
        }
       
        public IActionResult Private()
        {
            return View();
        }

        public IActionResult UserInfo(GitHubUserResponse userResponse)
        {
            return View(userResponse);
        }

        public async Task<IActionResult> Privacy([FromQuery] string code)
        {
            var access_token = await GetAcessToken(code);
            GitHubUserResponse? gituser = await GetUserInformation(access_token);
            return View("UserInfo", gituser);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<string> GetAcessToken(string code)
        {
            var response = await _github.GetAcessToken(new AcessTokenRequest()
            {
                client_id = "",
                client_secret = "",
                code = code
            });
            return HttpUtility.ParseQueryString(response).Get("access_token");

        }

        private async Task<GitHubUserResponse> GetUserInformation(string access_token)
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
            _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", $"{access_token}");

            var postResponse = await _httpClient.GetAsync("https://api.github.com/user");
            var re = await postResponse.Content.ReadAsStringAsync();
            var gituser = JsonConvert.DeserializeObject<GitHubUserResponse>(re);
            return gituser;
        }
    }
}