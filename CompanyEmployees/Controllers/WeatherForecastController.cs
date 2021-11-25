using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private ILoggerManager _logger;
        private readonly IRepositoryManager _repository;
        public WeatherForecastController(ILoggerManager logger, IRepositoryManager repository)
        {
            _logger = logger;
            _repository = repository;
        }


        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();

            //Example of logging
            //_logger.LogInfo("Info message from WeatherForecast controller");
            //_logger.LogDebug("Debug message from WeatherForecast controller");
            //_logger.LogWarn("Warn message from WeatherForecast controller");
            //_logger.LogError("Error message from WeatherForecast controller");

            var result = _repository.Company.FindAll(true);

            foreach (var item in result)
            {
                Console.WriteLine($"Company {item.Name} at {item.Address}");
            }

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
