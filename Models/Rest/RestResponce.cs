namespace SpotifyClone.Models.Rest
{
    public class RestResponce
    {
        public RestStatus Status { get; set; } = new();
        public RestMeta Meta { get; set; } = new();

        public Object? Data { get; set; }
    }
}
