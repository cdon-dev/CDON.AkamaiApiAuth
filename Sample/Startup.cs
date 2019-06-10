using System;
using AkamaiApiAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Sample
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.Configure<AkamaiAuthOptions>(_configuration.GetSection("AkamaiAuth"));
            services
                .AddHttpClient(
                    "AkamaiAuth", client => client.BaseAddress = _configuration.GetValue<Uri>("AkamaiApiUrl"))
                .AddHttpMessageHandler(sp => new AkamaiAuthHttpClientHandler(
                    sp.GetService<IOptions<AkamaiAuthOptions>>().Value));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}