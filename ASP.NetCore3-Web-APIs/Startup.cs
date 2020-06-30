using System.IO;
using ASP.NetCore3_Web_APIs.ActionFilters;
using ASP.NetCore3_Web_APIs.Extensions;
using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

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

            services.AddControllers(config =>
            {
                config.RespectBrowserAcceptHeader = true;   //telling the server to respect the Accept header
                config.ReturnHttpNotAcceptable = true;  //tells the server to return the 406 Not Acceptable status code if the client tries to negotiate for media type it doesn't suuport
                //config.Filters.Add(new GlobalFilterExample());
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

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=index}/{id?}");
            });
        }
    }
}
