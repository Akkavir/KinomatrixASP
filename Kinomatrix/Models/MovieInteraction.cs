public class MovieInteraction
{
    public int Id { get; set; }
    public string MovieId { get; set; }
    public int UserId { get; set; }
    public bool InWatchlist { get; set; }
    public int? Rating { get; set; } 

    public User User { get; set; }

    public DateTime DateTime { get; set; }

    public string Genres { get; set; }

}
