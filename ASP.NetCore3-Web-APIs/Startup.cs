using System.IO;
using ASP.NetCore3_Web_APIs.ActionFilters;
using ASP.NetCore3_Web_APIs.Extensions;
using AspNetCoreRateLimit;
using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using Repository;
using Repository.DataShaping;

namespace ASP.NetCore3_Web_APIs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "\\nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureRepositoryManager();
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            //services.AddScoped<ControllerFilterExample>();
            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();    //register DataShaper
            services.ConfigureVersioning();
            services.ConfigureResponseCaching();
            services.ConfigureHttpCacheHeaders();
            services.AddMemoryCache();  //Rate Limiting uses a memory cache to store its counters and rules
            services.ConfigureRateLimitingOptions();
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.ConfigureIdentity();
            services.ConfigureJWT(Configuration);
            services.AddScoped<IAuthenticationManager, AuthenticationManager>();
            services.ConfigureSwagger();

            services.AddControllers(config =>
            {
                config.RespectBrowserAcceptHeader = true;   //telling the server to respect the Accept header
                config.ReturnHttpNotAcceptable = true;  //tells the server to return the 406 Not Acceptable status code if the client tries to negotiate for media type it doesn't suuport
                //config.Filters.Add(new GlobalFilterExample());
                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });    //using CacheProfiles to extract ResponseCacheAttribute properties
            }).AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters()   //tell the server to support XML formatters.
            .AddCustomCSVFormatter();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                //supress the BadRequest error when ModelState is invalid
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.ConfigureExceptionHandler(logger);

            //app.UseHttpsRedirection();

            app.UseStaticFiles();
            
            app.UseCors("CorsPolicy");
            
            app.UseForwardedHeaders(new ForwardedHeadersOptions 
            {
                ForwardedHeaders = ForwardedHeaders.All
            });

            app.UseResponseCaching();

            app.UseHttpCacheHeaders();

            app.UseIpRateLimiting();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=index}/{id?}");
            });


            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                s.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1");
                s.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2");
            });
        }
    }
}
