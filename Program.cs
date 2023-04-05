using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using GitHubOAuth.Services;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
               .AddCookie()

               .AddOAuth("GitHub", "Github", o =>
               {
                   o.ClientId = builder.Configuration["github:clientid"];
                   o.ClientSecret = builder.Configuration["github:clientsecret"];
                   o.CallbackPath = new PathString("/home/Privacy");
                   o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                   o.TokenEndpoint = "https://github.com/login/oauth/access_token";
                   o.UserInformationEndpoint = "https://api.github.com/user";
                   o.ClaimsIssuer = "OAuth2-Github";
                   o.SaveTokens = true;
                   // Retrieving user information is unique to each provider.
                   o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                   o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                   o.ClaimActions.MapJsonKey("urn:github:name", "name");
                   o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
                   o.ClaimActions.MapJsonKey("urn:github:url", "url");
                   o.Events = new OAuthEvents
                   {
                       OnCreatingTicket = async context =>
                       {
                           // Get the GitHub user
                           var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                           request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                           request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                           var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                           response.EnsureSuccessStatusCode();

                           var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                           context.RunClaimActions(user.RootElement);
                       }
                   };
               });
builder.Services.AddRefitClient<IGitHubRequest>()
               .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://github.com/"));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
