using Microsoft.AspNetCore.Mvc;

namespace TerraformingMarsTrackerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TfmController : ControllerBase
    {

        private readonly ILogger<TfmController> _logger;

        public TfmController(ILogger<TfmController> logger)
        {
            _logger = logger;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateTime.Now.AddDays(index),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
    }
}