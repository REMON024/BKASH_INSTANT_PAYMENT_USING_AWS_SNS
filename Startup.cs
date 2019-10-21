using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NybSys.WASA.DAL;
using Microsoft.EntityFrameworkCore;
using NybSys.WASA.ExceptionLogManger.BLL;
using NybSys.WASA.Repository;
using NybSys.WASA.Account.BLL;
using NybSys.WASA.AuditLog.BLL;
using NybSys.WASA.BkashApi;

namespace NybSys.WASA.BkashApi
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMvc(config =>
            {
                // AWS SNS sends the request with content-type: text/plain
                // Even though the body is in JSON
                // Here we go through each input formatter
                foreach (var formatter in config.InputFormatters)
                {
                    // and if it's the JSON formatter
                    if (formatter.GetType() != typeof(JsonInputFormatter)) continue;

                    // add another supported media type "text/plain"
                    // so that incoming requests with this type, will also be processed as JSON
                    ((JsonInputFormatter)formatter).SupportedMediaTypes.Add(
                        Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/plain"));

                    // break the loop because there's no need to keep going through other formatters
                    break;
                }
            });
            services.AddDbContext<WasaDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("WASA_DB")), ServiceLifetime.Transient);

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                .Build());
            });
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddTransient<IExceptionLogBLLManager, ExceptionLogBLLManager>();
            services.AddTransient<ICardBLLManager, CardBLLManager>();
            services.AddTransient<IAccountBLLManager, AccountBLLManager>();
            services.AddTransient<IRechargeDetailBLLManager, RechargeDetailBLLManager>();
            services.AddTransient<ITransactionBLLManager, TransactionBLLManager>();
            services.AddTransient<IPaymentBLLManager, PaymentBLLManager>();
            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<IDashboardLogBLLManager, DashboardLogBLLManager>();
            services.AddTransient<IDeviceLogBLLManager, DeviceLogBLLManager>();
            services.AddTransient<IPumpRechargeTransactionBLLManager, PumpRechargeTransactionBLLManager>();




        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
