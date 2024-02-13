using Microsoft.AspNetCore.Authorization;

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
            .AddScheme<TwitchManagerAuthenticationOptions, TwitchManagerAuthenticationHandler>(TwitchManagerAuthenticationOptions.AuthenticationScheme, null);

            services.AddAuthorizationBuilder()
                .AddPolicy("Configured", policy => policy.Requirements.Add(new ConfiguredRequirement()));

            services.AddSingleton<IAuthorizationHandler, ConfiguredHandler>();

            return services;
        }

        public static void UseTwitchManagerAuth(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 403 || context.Response.StatusCode == 401)
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;

                    if(returnUrl == "/")
                    {
                        returnUrl = null;
                    }
                    var url = "/configure";

                    if(returnUrl != null)
                    {
                        url += $"?returnUrl={returnUrl}";
                    }

                    context.Response.Redirect(url);
                }
            });


            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
