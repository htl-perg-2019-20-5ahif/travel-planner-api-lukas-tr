using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TravelPlanner;

namespace TravelPlannerApi.Controllers
{
    [ApiController]
    [Route("api/travelPlan")]
    public class TravelPlanController : ControllerBase
    {
        private readonly ILogger<TravelPlanController> _logger;
        private readonly IHttpClientFactory _factory;

        public TravelPlanController(ILogger<TravelPlanController> logger, IHttpClientFactory factory)
        {
            _logger = logger;
            _factory = factory;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string from, [FromQuery] string to, [FromQuery] string start)
        {
            var client = _factory.CreateClient();
            var resp = await client.GetAsync("https://cddataexchange.blob.core.windows.net/data-exchange/htl-homework/travelPlan.json");
            resp.EnsureSuccessStatusCode();
            var routes = JsonSerializer.Deserialize<Route[]>(await resp.Content.ReadAsStringAsync());
            var finder = new ConnectionFinder(routes);
            var trip = finder.FindConnection(from, to, start);
            if (trip == null)
            {
                return NotFound();
            }

            return Ok(new { 
                depart = trip.FromCity,
                departureTime = trip.Leave,
                arrive = trip.ToCity,
                arrivalTime = trip.Arrive,
            });
        }
    }
}
