using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController
    {
        private readonly IService _service;
        public TestController(IService service)
        {
            _service = service;
        }

        [HttpGet(Name = "Get")]
        public string Get()
        {
            return _service.Method("hello");
        }
    }
}