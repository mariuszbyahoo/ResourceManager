using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ResourceManager.Data;
using ResourceManager.Data.Repos;
using ResourceManager.Data.Services;
using ResourceManager.Domain.Factories;
using ResourceManager.Services;

namespace ResourceManager
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
            services.AddControllers();

            services.AddDbContext<ManagerDbContext>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")
            ));
            services.AddScoped<ITenantRepo, TenantRepo>();
            services.AddScoped<IResourceRepo, ResourceRepo>();
            services.AddScoped<ILeasingDataRepo, LeasingDataRepo>();

            services.AddSingleton<IRemoveService, RemoveService>();
            services.AddSingleton<ILoggerService, Logger>();
            services.AddSingleton<IEmailService, EmailService>();

            services.AddSingleton<IResourceFactory, ResourceFactory>();
            services.AddSingleton<IResourceDataFactory, ResourceDataFactory>();
            services.AddSingleton<ITenantsFactory, TenantsFactory>();
            services.AddSingleton<ITenantDataFactory, TenantDataFactory>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
