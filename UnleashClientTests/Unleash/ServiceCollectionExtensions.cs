using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Unleash;
using Unleash.Strategies;

namespace UnleashClientTests.Unleash
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUnleash(this IServiceCollection serviceCollection, Action<UnleashSettings> configurator)
        {
            // This is to support the anonymous serviceCollection.AddSingleton<IUnleash, DefaultUnleash>() below.  If
            // DefaultUnleash's constructor asked for an IEnumerable<IStrategy> instead of an IStrategy[] this wouldn't
            // be necessary.  It shouldn't be a breaking change given that T[] : IEnumerable<T>.
            serviceCollection.AddSingleton(sp => sp.GetRequiredService<IEnumerable<IStrategy>>().ToArray());

            // ASP.NET Core will create a scope per HTTP request, message queue consumer code will typically create a
            // scope per message, timer-based services will create a scope per interval.   Disposing the scope disposes
            // the IDisposable scopes resolved.  We'd like to rely on DI's scope to provide isolation across
            // requests/messages/intervals, basically just new-ing up a Dictionary<string,string>() per request

            // In your docs, you say "If you are using Asp.Net the UnleashContextProvider will typically be a 'request
            // scoped' instance.".  I agree that makes sense here, and am trying to use DI to enforce it/handle the one
            // state per scope problem.  Also, my goal is for this extension method to work in anything that uses the
            // DI Scope pattern w/o any changes.
            serviceCollection.AddScoped<IUnleashContextProvider, ScopedDictionaryContextProvider>();

            // Since IUnleash should be a singleton, UnleashSettings must be as well.  However, the
            // UnleashContextProvider property is scoped.
            serviceCollection.AddSingleton<UnleashSettings>(sp =>
            {
                // In order to configure this, we are basically capturing a scoped dependency as a singleton.  This
                // instance will never exist within a scope and so it won't have anything to capture.

                var contextProvider = sp.GetRequiredService<IUnleashContextProvider>(); // Crash happens here

                var settings = new UnleashSettings
                {
                    UnleashContextProvider = contextProvider
                };

                configurator?.Invoke(settings);

                return settings;
            });

            serviceCollection.AddSingleton<IUnleash, DefaultUnleash>();
            /*
            Should be equivalent to:

            serviceCollection.AddSingleton<IUnleash>(sp =>
            {
                var unleashSettings = sp.GetRequiredService<UnleashSettings>();
                var strategies = sp.GetRequiredService<IEnumerable<IStrategy>>().ToArray();

                return new DefaultUnleash(unleashSettings, strategies);
            });
            */

            return serviceCollection;
        }
    }
}
