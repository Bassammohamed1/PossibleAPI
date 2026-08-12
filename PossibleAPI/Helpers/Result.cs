using GP_API.Models;

namespace GP_API.Helpers
{
    public class Result
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public object? Entity { get; set; }
    }
}