using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Thandizo.IdentityServer.Data.Migrations.AspNetIdentity;
using Thandizo.IdentityServer.Helpers;
using Thandizo.IdentityServer.Models;
using Thandizo.IdentityServer.Services;
using Thandizo.IdentityServer.Services.Messaging;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddControllersWithViews()
                    .AddNewtonsoftJson(options => options.UseCamelCasing(true))
                    .AddRazorRuntimeCompilation();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.HttpOnly = HttpOnlyPolicy.Always;
                //options.Secure = CookieSecurePolicy.Always;
            });

            services.AddDbContext<ThandizoIdentityDbContext>(options =>
                options.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly))
            );

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<ThandizoIdentityDbContext>()
            .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = $"{Configuration.GetValue<string>("Authority")}";
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.UserInteraction.LoginUrl = "/Account/Login";
                options.UserInteraction.LogoutUrl = "/Account/Logout";
                options.InputLengthRestrictions.Scope = 600;
                
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseNpgsql(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                options.EnableTokenCleanup = true;
            })
            .AddAspNetIdentity<ApplicationUser>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ISMSService, SMSService>();
            services.AddMvc();

            // not for production
            builder.AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });

                app.UseHttpsRedirection();
                app.UseXXssProtection(options => options.EnabledWithBlockMode());
                app.UseXContentTypeOptions();

                app.UseHsts(options =>
                {
                    options.MaxAge(days: 365);
                    options.IncludeSubdomains();
                });

                app.UseCsp(options => options.BlockAllMixedContent());
                app.UseReferrerPolicy(options => options.NoReferrer());
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                    await next();
                });
            }


            app.UseStaticFiles();
            app.UseRouting();
            app.UseCookiePolicy();
            

            app.UseAuthorization();
            app.UseIdentityServer();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
