using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using BTStatsCore.Models;

using BTStatsCorePopulator;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Hosting;

using NodaTime.TimeZones;

namespace BTStatsCore
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
            StatsProvider provider = new StatsProvider();
            provider.InitializeTask.ContinueWith((task) =>
            {
                Console.WriteLine("Stats Compiled");
            });

            services.AddSingleton<StatsProvider>(provider);
            services.AddSingleton<IHostedService>(provider);

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();

            services.AddMvc();
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<SerilogMiddleware>();

            app.UseDefaultFiles();
            app.UseResponseCompression();

            app.UseStaticFiles(new StaticFileOptions() {
              ServeUnknownFileTypes = true
            });
            app.UseMvc();
        }
    }
}
