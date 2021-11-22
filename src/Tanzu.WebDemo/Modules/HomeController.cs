using System;
using Microsoft.AspNetCore.Mvc;

namespace Tanzu.WebDemo.Modules
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [Route("/")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult Get()
        {
            return Redirect("swagger");
        }

        [HttpGet("/hello")]
        public string HelloWorld()
        {
            return "Hello Cool demo";
        }
    }
}