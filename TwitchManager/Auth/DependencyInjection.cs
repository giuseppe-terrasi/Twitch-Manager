using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

using TwitchManager.Auth.Requirements;

namespace TwitchManager.Auth
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTwitchManagerAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TwitchManagerAuthenticationOptions.AuthenticationScheme;
                options.DefaultChallengeScheme = TwitchManagerAuthenticationOptions.AuthenticationScheme;
            })
            //.AddScheme<TwitchManagerAuthenticationOptions, TwitchManagerAuthenticationHandler>(TwitchManagerAuthenticationOptions.AuthenticationScheme, null)
            .AddCookie(TwitchManagerAuthenticationOptions.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "TwitchManagerAuth";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.LoginPath = "/login";
            })
            .AddCookie(TwitchManagerAuthenticationOptions.BotAuthenticationScheme, options =>
            {
                options.Cookie.Name = "BotOpenId";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.ForwardChallenge = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddOpenIdConnect(options =>
            {
                options.SignInScheme = TwitchManagerAuthenticationOptions.BotAuthenticationScheme;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("user:read:chat");
                options.Scope.Add("user:write:chat");
                options.Scope.Add("user:bot");
                //options.SaveTokens = true;
                options.ClientId = configuration["Config:ClientId"];
                options.ClientSecret = configuration["Config:ClientSecret"];
                options.Authority = configuration["Config:TokenUrl"];
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.CallbackPath = "/signin-oidc";
                options.SignedOutCallbackPath = "/signout-callback-oidc";
                options.SignedOutRedirectUri = "/";
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;
                options.ResponseType = "code";
            });

            services.AddAuthorizationBuilder()
                .AddPolicy(TwitchManagerAuthenticationOptions.ConfiguredPolicy, policy => policy.Requirements.Add(new ConfiguredRequirement()));

            services.AddSingleton<IAuthorizationHandler, ConfiguredHandler>();

            return services;
        }

        public static void UseTwitchManagerAuth(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.Use(async (context, next) =>
            {
                await next();

                if(context.Request.Path.Value?.Contains("webhook") ?? false)
                {
                    return;
                }

                if(context.Response.StatusCode != 401 && context.Response.StatusCode != 403)
                {
                    return;
                }

                var returnUrl = context.Request.Path + context.Request.QueryString;

                if (returnUrl == "/")
                {
                    returnUrl = null;
                }

                string url;

                if (context.Response.StatusCode == 403)
                {
                    url = "/settings";

                }
                else
                {
                    url = "/login";
                }

                if (returnUrl != null)
                {
                    url += $"?returnUrl={returnUrl}";
                }

                context.Response.Redirect(url);
            });


            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
