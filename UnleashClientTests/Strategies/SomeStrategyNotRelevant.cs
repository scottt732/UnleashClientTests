using System.Collections.Generic;
using Unleash;
using Unleash.Strategies;

namespace UnleashClientTests.Strategies
{
    public class SomeStrategyNotRelevant : IStrategy
    {
        public string Name { get; } = "NotRelevant";
        public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context) => true;
    }
}
