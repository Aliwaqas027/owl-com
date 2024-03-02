using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OwlApi.Exceptions;
using OwlApi.Helpers;
using System;
using System.IO;
using System.Security.Authentication;

namespace OwlApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
              .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSpaStaticFiles();

            var envConnectionString =  Configuration["ConnectionStrings:DefaultConnection"];
            services.AddDbContext<OwlApiContext>(options =>
              options.UseNpgsql(envConnectionString));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(async options =>
                {
                    options.SecurityTokenValidators.Clear();
                    options.SecurityTokenValidators.Add(new JWTSecurityTokenValidator(services.BuildServiceProvider().GetService<IHttpContextAccessor>(), Configuration));
                    options.Events = new JwtBearerEvents()
                    {
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            return c.Response.WriteAsync(c.Exception.Message);
                        },
                    };
                });

            services.AddSingleton<SMSClient>();
            services.AddSingleton<EmailClient>();
            services.AddSingleton<EmailTemplates>();
            services.AddTransient<ReservationHelper>();
            services.AddSingleton<KeycloakClient>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:4201",
                                            "http://localhost:4202",
                                            "http://localhost:4203",
                                            "http://localhost:5002",
                                            "http://localhost:5003",
                                            "http://*.omniopti.eu")
                                            .AllowAnyMethod()
                                            .AllowAnyHeader();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/html";

                var exceptionHandlerPathFeature =
            context.Features.Get<IExceptionHandlerPathFeature>();

                Exception error = exceptionHandlerPathFeature?.Error;
                if (error is ModelNotFoundException)
                {
                    await context.Response.WriteAsync("Not found!");
                }
                else if (error is AuthenticationException)
                {
                    await context.Response.WriteAsync("Not authorized!");
                }
                else if (error is IncorrectRequest)
                {
                    await context.Response.WriteAsync("Incorrect input!");
                }
                else
                {
                    await context.Response.WriteAsync(error.Message);
                }
            });
                });
                app.UseHsts();
            }

            app.UseAuthentication();
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ErrorLoggingMiddleware>();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
                    context.Context.Response.Headers.Add("Expires", "-1");
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller}/{action=Index}/{id?}/{ending?}");
            });
        }
    }
}
