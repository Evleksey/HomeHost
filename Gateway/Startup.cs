using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Gateway.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry.AspNetCore;

namespace Gateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
                                .Build();

            Auth = new GoogleJwt("scope", "api-477@homehost-315909.iam.gserviceaccount.com", "homehost-315909-9e21f4f2bb82.p12");
        }

        public IConfiguration Configuration { get; }

        public IGoogleOAuth2 Auth { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var x  = Auth.RequestAccessTokenAsync().Result;

            services.AddScoped<IGoogleOAuth2>(_ => Auth);

            services.AddCors();
            //    (options =>
            //{
            //    options.AddDefaultPolicy(
            //        builder =>
            //        {
            //            builder.WithOrigins("http://localhost:8080");
            //        });
            //});

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GateWay", Version = "v1" });
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            // ????????, ????? ?? ?????????????? ???????? ??? ????????? ??????
                            ValidateIssuer = true,
                            // ??????, ?????????????? ????????
                            ValidIssuer = AuthOptions.ISSUER,

                            // ????? ?? ?????????????? ??????????? ??????
                            ValidateAudience = true,
                            // ????????? ??????????? ??????
                            ValidAudience = AuthOptions.AUDIENCE,
                            // ????? ?? ?????????????? ????? ?????????????
                            ValidateLifetime = true,

                            // ????????? ????? ????????????
                            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                            // ????????? ????? ????????????
                            ValidateIssuerSigningKey = true,
                        };
                    });
            services.AddControllersWithViews();

            // This is required for HTTP client integration
            services.AddHttpClient();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway v1"));
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway v1"));
            }

            app.UseCors(builder => builder.AllowAnyOrigin());

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSentryTracing();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
