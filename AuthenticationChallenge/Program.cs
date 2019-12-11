using AuthenticationChallenge.Data;
using AuthenticationChallenge.Models;
using AuthenticationChallenge.Settings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthenticationChallenge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHost host = BuildWebHost(args);
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
                UserManager<ApplicationUser> usermanager = services.GetRequiredService<UserManager<ApplicationUser>>();
                AuthenticationChallengeConstants.AddInitialData(context, usermanager);
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://*:5000")
                .Build();
        }
    }
}
