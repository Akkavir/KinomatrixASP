using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kinomatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private static readonly Random _random = new Random();

        public MoviesController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetMovie(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Missing 'title' query parameter");
            }

            bool demo = true; // assuming this is controlled elsewhere
            if (demo)
            {
                string jsonFile = $"demodata/demo{_random.Next(1, 6)}.json";

                if (!System.IO.File.Exists(jsonFile))
                {
                    return NotFound("Demo data file not found.");
                }

                string json = await System.IO.File.ReadAllTextAsync(jsonFile);
                return Content(json, "application/json");
            }
            else
            {
                string apiKey = _config["OMDB:ApiKey"]; // Fetch from appsettings.json
                string url = $"http://www.omdbapi.com/?apikey={apiKey}&t={title}&plot=full";

                try
                {
                    string response = await _httpClient.GetStringAsync(url);
                    var movieData = JsonConvert.DeserializeObject<dynamic>(response);

                    if (movieData.Response == "False")
                    {
                        return NotFound(movieData.Error.ToString());
                    }

                    return Content(response, "application/json");
                }
                catch (HttpRequestException ex)
                {
                    return StatusCode(500, $"Error fetching movie data: {ex.Message}");
                }
            }
        }
    }
}
