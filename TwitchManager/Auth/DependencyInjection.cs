using Microsoft.AspNetCore.Authorization;

using System;

using TwitchManager.Auth.Requirements;

namespace TwitchManager.Auth
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTwitchManagerAuth(this IServiceCollection services)
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
