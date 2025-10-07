namespace SpotifyClone.Models.Rest
{
    public class RestStatus
    {
        public String Phrase { get; set; } = "Ok";
        public int Code { get; set; } = 200;
        public bool IsOk { get; set; } = true;

        public static readonly RestStatus Status200 = new() { IsOk = true, Code = 200, Phrase = "OK" };
        public static readonly RestStatus Status400 = new() { IsOk = false, Code = 400, Phrase = "Bad Request" };
        public static readonly RestStatus Status401 = new() { IsOk = false, Code = 401, Phrase = "Unauthorized" };
        public static readonly RestStatus Status403 = new() { IsOk = false, Code = 403, Phrase = "Forbidden access" };
        public static readonly RestStatus Status404 = new() { IsOk = false, Code = 404, Phrase = "Not Found" };
        public static readonly RestStatus Status409 = new() { IsOk = false, Code = 409, Phrase = "Conflict" };
    }
}
