using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Unleash;
using UnleashClientTests.AspNetCore.Mvc;

namespace UnleashClientTests.AspNetCore.Controllers
{
    public class TestController : Controller
    {
        private IUnleash Unleash { get; }

        public TestController(IUnleash unleash)
        {
            Unleash = unleash;
        }

        [Route("Test")]
        public IActionResult Get()
        {
            if (!Unleash.IsEnabled("x"))
            {
                return NotFound();
            }

            return Ok("Ok");
        }

        [Route("Keys")]
        public ActionResult<Dictionary<string, string>> Keys(IUnleashContextProvider contextProvider)
        {
            return contextProvider.Context.Properties;
        }
    }
}
