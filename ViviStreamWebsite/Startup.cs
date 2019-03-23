using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViviStreamWebsite.Data;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Microsoft.AspNetCore.Http;

namespace ViviStreamWebsite
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
            try
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.AllowedUserNameCharacters = null;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

                services.AddAuthentication().AddGoogle("YouTube", googleOptions =>
                {
                    googleOptions.ClientId = Configuration.GetSection("Google")["id"];
                    googleOptions.ClientSecret = Configuration.GetSection("Google")["secret"];
                    googleOptions.Scope.Add("https://www.googleapis.com/auth/youtube.readonly");
                    googleOptions.SaveTokens = true;
                    googleOptions.AccessType = "offline";
                    googleOptions.AuthorizationEndpoint += "?prompt=consent";
                    googleOptions.Events.OnCreatingTicket = (ctx) =>
                    {
                        List<AuthenticationToken> tokens = ctx.Properties.GetTokens() as List<AuthenticationToken>;
                        ctx.Properties.StoreTokens(tokens);
                        return Task.CompletedTask;
                    };
                });

                // Add application services.
                services.AddTransient<IEmailSender, EmailSender>();

                services.AddMvc();
                services.AddSingleton<IConfiguration>(Configuration);

                services.ConfigureApplicationCookie(
                    options => options.LoginPath = "/Home");
            }
            catch (Exception ex)
            {

            }
        }

        //roleName = "Moderator", email = "luis.beltran@itcelaya.edu.mx" por ejemplo
        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Moderator", "Friend", "User", "Owner" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                //creating the roles and seeding them to the database
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private async Task AddUserToRole(IServiceProvider serviceProvider, string role, string email)
        {
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var _user = await UserManager.FindByEmailAsync(email);
            await UserManager.AddToRoleAsync(_user, role);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider  serviceProvider)
        {
            try
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

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });

                //CreateRoles(serviceProvider).Wait();
                //AddUserToRole(serviceProvider, "Moderator", "correo@mail.net").Wait();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
