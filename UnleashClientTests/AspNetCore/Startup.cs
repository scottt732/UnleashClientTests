using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Unleash;
using Unleash.Strategies;
using UnleashClientTests.AspNetCore.Mvc;
using UnleashClientTests.Strategies;
using UnleashClientTests.Unleash;

namespace UnleashClientTests.AspNetCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IStrategy, SomeStrategyNotRelevant>();
            services.AddUnleash(settings => { settings.AppName = "MyWebApplication"; });

            services.AddMvc(options =>
            {
                // Hook up an MVC-layer filter that can add MVC-level concepts to the
                // IUnleashContextProvider.Context.Properties
                options.Filters.Add<UnleashMvcActionFilter>();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            // Middleware at the ASP.NET Core layer can alter the request-scoped IUnleashContextProvider
            app.Use((ctx, next) =>
            {
                var ctxProvider = ctx.RequestServices.GetRequiredService<IUnleashContextProvider>();
                ctxProvider.Context.Properties["Method"] = ctx.Request.Method;
                ctxProvider.Context.Properties["Path"] = ctx.Request.Path;
                return next();
            });

            // ActionFilters in ASP.NET Core MVC (higher-level than middleware) can alter the same request-scoped
            // IUnleashContextProvider on the same request.  See UnleashMvcActionFilter.
            app.UseMvcWithDefaultRoute();

            // An ASP.NET Core MVC controller action should see all of the properties (Method, Path, ActionDisplayName).
            // Lower-level middleware would only see Method + Path
        }
    }
}
