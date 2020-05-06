using AuthenticationChallenge.Data;
using AuthenticationChallenge.Models;
using AuthenticationChallenge.Services;
using AuthenticationChallenge.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthenticationChallenge
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(AuthenticationChallengeConstants.DatabaseName));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            });
            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();

            services.AddDistributedMemoryCache();
            services.AddSession();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            RoleManager<IdentityRole> roleManager = app.ApplicationServices.GetService<RoleManager<IdentityRole>>();
            UserManager<ApplicationUser> userManager = app.ApplicationServices.GetService<UserManager<ApplicationUser>>();
            List<Task> tasks = new List<Task>();
            foreach(string role in AuthorizationRoles.AllRoles)
            {
                tasks.Add(roleManager.CreateAsync(new IdentityRole(role)));
            }
            Task.WhenAll(tasks.ToArray()).ContinueWith(roleTask =>
            {
                ApplicationUser adminUser = new ApplicationUser()
                {
                    UserName="admin@test.com",
                    Email="admin@test.com",
                    Answer1="a",
                    Answer2="a",
                    SocialSecurityNumber="111111111",
                    AccountNumber="1"                    
                };
                userManager.CreateAsync(adminUser, "password").ContinueWith(createUserTask =>
                {
                    userManager.FindByNameAsync("admin@test.com").ContinueWith(findTask =>
                    {
                        userManager.AddToRoleAsync(findTask.Result, AuthorizationRoles.Administrator);
                    });
                });
            });

        }
    }
}
