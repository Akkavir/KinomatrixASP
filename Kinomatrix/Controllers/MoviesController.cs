using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kinomatrix.Models;
using System.Security.Claims; // Dodaj referencję do modeli


namespace Kinomatrix.Controllers
{
    public class MoviesController : Controller // 🔹 Zmień z ControllerBase na Controller
    {
        bool demo = true; /// DEMO
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private static readonly Random _random = new Random();
        private readonly AppDbContext _context;



        public MoviesController(HttpClient httpClient, IConfiguration config, AppDbContext context)
        {
            _httpClient = httpClient;
            _config = config;
            _context = context; // <--- to był brakujący kawałek
        }

        public static int? CalculateFullRating(string jsonResponse)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
            int validRatingCount = 0;
            double totalRating = 0;
           
            if (data != null && data.Ratings[0].Value != null)
            {
                string ratingValue = data.Ratings[0].Value;
                string[] parts = ratingValue.Split('/');


                string normalizedRating = parts[0].Replace('.', ','); // For locales that use comma
                if (float.TryParse(normalizedRating, out float rating))
                {
                    int imdratingtenth = (int)Math.Round(rating * 10, MidpointRounding.AwayFromZero);
                    totalRating += imdratingtenth;
                    validRatingCount++;
                    Console.WriteLine(imdratingtenth); // Output: 36
                }
                else
                {
                    Console.WriteLine("Failed to parse the rating value: " + parts[0]);
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
        public string GenerateStars(decimal? rating)
        {
            if (!rating.HasValue)
                return new string('☆', 10); // or throw, depending on your needs

            // Normalize rating to 0–10 scale and round to nearest whole number
            int filledStars = (int)Math.Round(rating.Value, MidpointRounding.AwayFromZero);

            // Clamp value to [0, 10] to prevent overflow
            filledStars = Math.Min(10, Math.Max(0, filledStars));

            int emptyStars = 10 - filledStars;

            return new string('★', filledStars) + new string('☆', emptyStars);
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
            model.AverageRating = (CalculateFullRating(response)) / 10;
            model.AllRatings = Ratings(response);
            model.Stars = GenerateStars((int)Math.Round((decimal)model.AverageRating));
            return View("MovieDetails", model);
        }


        [HttpPost]
        public IActionResult SaveInteraction([FromBody] InteractionDto data)
        {
            int? userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : (int?)null;
            if (userId == null)
            {
                Console.WriteLine("Session userId is null");
                return Unauthorized();
            }

            Console.WriteLine($"userId: {userId}, movieId: {data.MovieId}, inWatchlist: {data.InWatchlist}, rating: {data.Rating}, genre: {data.Genres}");

            var interaction = _context.MovieInteractions
                .FirstOrDefault(m => m.UserId == userId && m.MovieId == data.MovieId);

            if (interaction == null)
            {
                interaction = new MovieInteraction
                {
                    UserId = userId.Value,
                    MovieId = data.MovieId ?? "0",
                    InWatchlist = data.InWatchlist,
                    Rating = data.Rating,
                    DateTime = DateTime.Now,
                    Genres = data.Genres
                };
                _context.MovieInteractions.Add(interaction);
            }
            else
            {
                interaction.InWatchlist = data.InWatchlist;
                interaction.Rating = data.Rating;
                interaction.DateTime = DateTime.Now;
                interaction.Genres = data.Genres;
            }

            _context.SaveChanges();
            return Ok();
        }


    }
}


