public class InteractionDto
{
    public string MovieId { get; set; }
    public bool InWatchlist { get; set; }
    public int? Rating { get; set; }

    public List<string> Genres { get; set; } = new List<string>();
}
