using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GroupProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GroupProject
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
            string connectionString = Configuration.GetConnectionString("DB_Connection");
            services.AddScoped<DB_Context>();

            services.AddAuthentication("Fitness247UserAuthentication")
                .AddCookie("Fitness247UserAuthentication", options =>
                {
                    options.AccessDeniedPath = new PathString("/");
                    options.Cookie = new CookieBuilder
                    {
                        //Domain = "",
                        HttpOnly = true,
                        Name = ".Fitness.24x7.Cookie",
                        Path = new PathString("/"),
                        SameSite = SameSiteMode.Lax,
                        SecurePolicy = CookieSecurePolicy.SameAsRequest
                    };
                    options.LoginPath = new PathString("/");
                    options.ReturnUrlParameter = "RequestPath";
                    options.SlidingExpiration = true;
                })
                .AddCookie("Fitness247AdminAuthentication", options =>
                {
                    options.AccessDeniedPath = new PathString("/admin");
                    options.Cookie = new CookieBuilder
                    {
                        //Domain = "",
                        HttpOnly = true,
                        Name = ".Fitness.24x7.Admin.Cookie",
                        Path = new PathString("/admin"),
                        SameSite = SameSiteMode.Lax,
                        SecurePolicy = CookieSecurePolicy.SameAsRequest
                    };
                    options.LoginPath = new PathString("/admin/login");
                    options.ReturnUrlParameter = "RequestPath";
                    options.SlidingExpiration = true;
                });
            services.AddRouting(options => options.LowercaseUrls = true);
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.DefaultRequestCulture.UICulture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
                options.DefaultRequestCulture.Culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            });
            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

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
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseRequestLocalization();
            app.UseAuthentication();
            app.UseStaticFiles();
            

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}