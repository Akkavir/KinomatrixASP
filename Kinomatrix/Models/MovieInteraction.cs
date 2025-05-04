public class MovieInteraction
{
    public int Id { get; set; }
    public string MovieId { get; set; } // imdbID
    public int UserId { get; set; }
    public bool InWatchlist { get; set; }
    public int? Rating { get; set; } // 1-10, null if not rated

    public User User { get; set; }
}
