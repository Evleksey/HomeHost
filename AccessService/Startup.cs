using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccessService
{
    public class Startup
    {
        public Startup()
        {
            Keys = new Keys("api-477@homehost-315909.iam.gserviceaccount.com");
        }
        public IKeys Keys { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _ = Keys.GetKeys();
            services.AddScoped<IKeys>(_ => Keys);

            services.AddGrpc();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.SecurityTokenValidators.Clear();
                        options.SecurityTokenValidators.Add(new GoogleTokenValidator(Keys));
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            // укзывает, будет ли валидироваться издатель при валидации токена
                            ValidateIssuer = true,
                            // строка, представляющая издателя
                            ValidIssuer = "api-477@homehost-315909.iam.gserviceaccount.com",

                            // будет ли валидироваться потребитель токена
                            ValidateAudience = true,
                            // установка потребителя токена
                            ValidAudience = "acessserver",
                            // будет ли валидироваться время существования
                            ValidateLifetime = true,
                            
                            // валидация ключа безопасности
                            ValidateIssuerSigningKey = true,
                        };
                    });
            
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints((Action<Microsoft.AspNetCore.Routing.IEndpointRouteBuilder>)(endpoints =>
            {
                GrpcEndpointRouteBuilderExtensions.MapGrpcService<LogonService>(endpoints);

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            }));
        }
    }
}
