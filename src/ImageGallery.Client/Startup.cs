﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ImageGallery.Client.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

namespace ImageGallery.Client
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            // Add an authorization policy
            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                    "CanOrderFrame",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.RequireClaim("country", "be");
                        policyBuilder.RequireClaim("subscriptionlevel", "PayingUser");
                    });
            });


            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
            }

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationScheme = "Cookies",
                AccessDeniedPath = "/Authorization/AccessDenied"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions {
                AuthenticationScheme = "oidc",
                Authority = "https://localhost:44387/",
                RequireHttpsMetadata = true,
                ClientId = "imagegallaryclient",
                Scope = { "openid", "profile", "address", "roles", "imagegalleryapi",
                    "subscriptionlevel", "country", "offline_access"
                },
                ResponseType = "code id_token",
                //CallbackPath = new PathString("...")
                SignInScheme = "Cookies",
                SaveTokens = true,
                ClientSecret = "secret",
                GetClaimsFromUserInfoEndpoint = true,
                Events = new OpenIdConnectEvents()  {
                    OnTokenValidated = tokenValidatedContext => {
                        var identity = tokenValidatedContext.Ticket.Principal.Identity as ClaimsIdentity;

                        var subjectClaim = identity.Claims.FirstOrDefault(x => x.Type == "sub");

                        var newClaimsIdentity = new ClaimsIdentity(
                            tokenValidatedContext.Ticket.AuthenticationScheme, 
                            "given_name", 
                            "role");
                        newClaimsIdentity.AddClaim(subjectClaim);

                        tokenValidatedContext.Ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(
                            new ClaimsPrincipal(newClaimsIdentity),
                            tokenValidatedContext.Ticket.Properties,
                            tokenValidatedContext.Ticket.AuthenticationScheme);

                        return Task.FromResult(0);
                    },

                    OnUserInformationReceived = userInfomationReceivedContext => {

                        userInfomationReceivedContext.User.Remove("address");

                        return Task.FromResult(0);
                    }
                }
            });
            
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }         
    }
}
