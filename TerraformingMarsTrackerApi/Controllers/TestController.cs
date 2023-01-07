using Microsoft.AspNetCore.Mvc;

namespace TerraformingMarsTrackerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {

        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "TestDb")]
        public async Task<Root> TestDb()
        {
            var cdb = new DbClient();
            var item = await cdb.Connect();
            return item;
        }
    }
}