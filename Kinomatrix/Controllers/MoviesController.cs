using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kinomatrix.Models; // Dodaj referencję do modeli


namespace Kinomatrix.Controllers
{
    public class MoviesController : Controller // 🔹 Zmień z ControllerBase na Controller
    {
        bool demo = true; /// DEMO
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private static readonly Random _random = new Random();




        public MoviesController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public static int? CalculateFullRating(string jsonResponse)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            int validRatingCount = 0;
            double totalRating = 0;

            if (data != null && data.imdbRating != null)
            {
                if (double.TryParse(data.imdbRating.ToString(), out double imdbRating))
                {
                    totalRating += imdbRating * 10;
                    validRatingCount++;
                }
                else
                {
                    // Handle invalid IMDb rating format
                    Console.WriteLine("Invalid IMDb rating format.");
                }
            }

            // Process Metascore rating (already on 0-100 scale)
            if (data != null && data.Metascore != null)
            {
                if (double.TryParse(data.Metascore.ToString(), out double metascore))
                {
                    totalRating += metascore;
                    validRatingCount++;
                }
                else
                {
                    // Handle invalid Metascore format
                    Console.WriteLine("Invalid Metascore format.");
                }
            }

            // Process Rotten Tomatoes rating (format: "XX%")
            if (data.Ratings != null && data.Ratings.Count > 1)
            {
                var rottenTomatoesRating = data.Ratings[1]?.Value;
                if (rottenTomatoesRating != null)
                {
                    string ratingString = rottenTomatoesRating.ToString().Replace("%", "");
                    if (double.TryParse(ratingString, out double parsedRating))
                    {
                        totalRating += parsedRating;
                        validRatingCount++;
                    }
                }
            }

            // Calculate average only if we have at least one valid rating
            if (validRatingCount == 0)
            {
                return null; // No valid ratings available
            }

            int averageRating = (int)Math.Round(totalRating / validRatingCount);

            // Optional logging
            Console.WriteLine($"Average rating: {averageRating} (from {validRatingCount} sources)");

            return averageRating;
        }

        public static string Ratings(string jsonResponse)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            data.Ratings[0].Source = "IMDb";
            return string.Join(", ", ((IEnumerable<dynamic>)data.Ratings).Select(r => $"{r.Source}: {r.Value}"));
        }


        public async Task<IActionResult> GetMovie(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Missing 'title' query parameter");
            }

            string response;
            if (demo)
            {
                string jsonFile = $"demodata/demo{_random.Next(1, 6)}.json";

                if (!System.IO.File.Exists(jsonFile))
                {
                    return NotFound("Demo data file not found.");
                }

                response = await System.IO.File.ReadAllTextAsync(jsonFile);
            }
            else
            {
                string apiKey = _config["OMDB:ApiKey"]; // Fetch from appsettings.json
                string url = $"http://www.omdbapi.com/?apikey={apiKey}&t={title}&plot=full";

                try
                {
                    response = await _httpClient.GetStringAsync(url);
                    var movieData = JsonConvert.DeserializeObject<dynamic>(response);

                    if (movieData.Response == "False")
                    {
                        return NotFound(movieData.Error.ToString());
                    }
                }
                catch (HttpRequestException ex)
                {
                    return StatusCode(500, $"Error fetching movie data: {ex.Message}");
                }
            }

            var model = JsonConvert.DeserializeObject<MovieDetailsViewModel>(response);
            model.AverageRating = CalculateFullRating(response);
            model.AllRatings = Ratings(response);
            return View("MovieDetails", model);
        }
    }
}


