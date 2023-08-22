using Helper.Data;
using Helper.Interface;
using Helper.Model;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;

using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using System.IO;

using Helper.Report;
using Helper.Hub_Config;

namespace FTC
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
            services.AddSignalR();
            services.AddControllers();
            services.AddDbContext<DataConnection>(Options => Options.UseSqlServer(Configuration.GetConnectionString("FTCConnection")));
            services.AddCors();
            services.AddScoped<ISNQEnquiry, SNQEnquiry>();
            services.AddScoped<IMSDEnquiry, MSDEnquiry>();
            services.AddScoped<ILoginAuth,LoginAuth>();
            services.AddScoped<IUser, UserDtls>();
            services.AddScoped<ITaskDtl, TaskClass>();
            services.AddScoped<ISNQReport,SNQReport>();
            services.AddScoped<IMSDReport, MSDReport>();
            services.AddScoped<IMLMaster, MLMaster>();
            services.AddScoped<IMESPASEvent, MESPASEvent>();
            services.AddScoped<ISourceType, SourceType>();
            services.AddScoped<IMESPASEnquiry, MESPASEnquiry>();
            services.AddScoped<IQuotationSubmitReport, QuotationSubmitReport>();
            services.AddScoped<IChangeCustomerSettings, ChangeCustomerSettings>();
            SetupJWTServices(services);
            //services.AddCors(options =>
            //{
            //    // this defines a CORS policy called "default"
            //    options.AddPolicy("default", policy =>
            //    {
            //        policy.WithOrigins("http://localhost:3000")
            //            .AllowAnyHeader()
            //            .AllowAnyMethod();
            //    });
            //});

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
           
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(x => x
               .AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // allow any origin
               .AllowCredentials()); // allow credentials
          
            // app.UseCors("default");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            //loggerFactory.AddLog4Net();
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllers();
                endpoints.MapHub<SignalRHub>("/notify");

            });

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = context =>
            //    {
            //        if (context.File.Name == "index.html")
            //        {
            //            context.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            //            context.Context.Response.Headers.Add("Expires", "-1");
            //        }
            //    }
            //});

        }

        private void SetupJWTServices(IServiceCollection services)
        {
            string key = "ftc_secret_key_12345"; //this should be same which is used while creating token      
            var issuer = "http://mysite.com";  //this should be same which is used while creating token  

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
          .AddJwtBearer(options =>
          {
              options.TokenValidationParameters = new TokenValidationParameters
              {
                  // ValidateIssuer = true,
                  ValidateIssuer = true,
                  // ValidateAudience = true,
                  ValidateAudience = true,
                  ValidateIssuerSigningKey = true,
                  ValidIssuer = issuer,
                  ValidAudience = issuer,
                  ValidateLifetime = true,
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                  // ClockSkew =  TimeSpan.FromSeconds(1),
                  LifetimeValidator = TokenLifetimeValidator.Validate,

              };

              options.Events = new JwtBearerEvents
              {
                  OnAuthenticationFailed = context =>
                  {
                      if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                      {
                          context.Response.Headers.Add("Token-Expired", "true");
                      }
                      return Task.CompletedTask;
                  }
              };
          });
        }

        public static class TokenLifetimeValidator
        {
            public static bool Validate(
                DateTime? notBefore,
                DateTime? expires,
                SecurityToken tokenToValidate,
                TokenValidationParameters @param
            )
            {
                return (expires != null && expires > DateTime.UtcNow);
            }
        }


    }

}
