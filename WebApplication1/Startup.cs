using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;

namespace WebApplication1
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
            services.Configure<AzureAdOptions>(Configuration.GetSection("AzureAd"));
            services.AddSingleton<IConfidentialClientApplication>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AzureAdOptions>>().Value;
                return ConfidentialClientApplicationBuilder.Create(options.ClientId)
                    .WithClientSecret(options.ClientSecret)
                    .WithAuthority(new Uri($"{options.Instance}{options.TenantId}"))
                    .Build();
            });

            services.AddSingleton<GraphServiceClient>(provider =>
            {
                var clientApplication = provider.GetRequiredService<IConfidentialClientApplication>();
                return new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    var result = await clientApplication.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" })
                        .ExecuteAsync();
                    requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);
                }));
            });

            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
