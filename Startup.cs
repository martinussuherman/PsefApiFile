using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace PsefApiFile
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ApiHelper.ReadConfiguration(Configuration);
            services.AddControllers();
            ConfigureSwaggerGen(services);
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // base-address of your identityserver
                    options.Authority = ApiHelper.Authority;

                    // if you are using API resources, you can specify the name here
                    options.Audience = ApiHelper.Audience;
                });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            string basePath = Configuration.GetValue<string>("BasePath");

            app
                .UsePathBase(basePath)
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.OAuthClientId(Configuration.GetValue<string>("ClientId"));
                    options.OAuthAppName("PsefApiFile Swagger");
                    options.OAuthUsePkce();
                    options.SwaggerEndpoint(
                        $"{basePath}/swagger/v1/swagger.json",
                        "Psef File API V1");
                });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureSwaggerGen(IServiceCollection services)
        {
            var authCodeFlow = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{ApiHelper.Authority}/connect/authorize"),
                TokenUrl = new Uri($"{ApiHelper.Authority}/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { ApiHelper.Audience, "Api access" }
                }
            };

            services.AddSwaggerGen(options =>
            {
                // integrate xml comments
                // options.IncludeXmlComments(XmlCommentsFilePath);

                // Set the comments path for the Swagger JSON and UI.
                // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // c.IncludeXmlComments(xmlPath);

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                options.OperationFilter<AuthorizeCheckOperationFilter>();
                options.AddSecurityDefinition(
                    ApiInfo.SchemeOauth2,
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = authCodeFlow
                        }
                    });
                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = ApiInfo.SchemeOauth2
                                }
                            },
                            new[]
                            {
                                ApiHelper.Audience
                            }
                        }
                    });
            });
        }
    }
}
