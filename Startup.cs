using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using maple_web_api.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using maple_web_api.Services;

namespace maple_web_api
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
            services.AddMvc().AddMvcOptions(opt =>
            {
                opt.EnableEndpointRouting = false;
                opt.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

            }).AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddScoped<IInsuranceInfoRepository, InsuranceInfoRepository>();
            services.AddDbContext<InsuranceInfoContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("MapleDB")));

            services.AddDbContext<CustomerContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("MapleDB")));

            services.AddDbContext<CoveragePlanContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("MapleDB")));

            services.AddDbContext<RateChartContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("MapleDB")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.UseStatusCodePages();
            app.UseMvc();
        }
    }
}
