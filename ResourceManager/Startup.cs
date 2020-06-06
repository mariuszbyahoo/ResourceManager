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
using ResourceManager.Domain.Factories;

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
            services.AddTransient<ITenantRepo, TenantRepo>();
            services.AddTransient<IResourceRepo, ResourceRepo>();

            services.AddSingleton<IResourceFactory, ResourceFactory>();
            services.AddSingleton<ITenantsFactory, TenantsFactory>();
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
