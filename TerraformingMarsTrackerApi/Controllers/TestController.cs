using Microsoft.AspNetCore.Mvc;
using TerraformingMarsTrackerApi.Models;

namespace TerraformingMarsTrackerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private CosmosDbClient _db;

        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger, CosmosDbClient db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet]
        [Route("GetGame/{gameCode}")]
        public async Task<GameState> TestDb(string gameCode)
        {
            var (success, item) = await _db.Get(gameCode);
            if (success && item != null)
            {
                return item;
            }
            throw new Exception();
        }
    }
}