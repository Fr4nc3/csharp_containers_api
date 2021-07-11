using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

namespace fr4nc3.com.containers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args)
                  .ConfigureAppConfiguration((hostContext, builder) =>
                   {

                       if (hostContext.HostingEnvironment.IsDevelopment())
                       {
                           builder.AddUserSecrets<Program>();
                       }
                   })
                  .Build();

            // Get the dependency injection for creating services
            using (var scope = host.Services.CreateScope())
            {
                // Get the service provider so services can be called
                var services = scope.ServiceProvider;

            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
