namespace GP_API.Models
{
    public class AuthModel
    {
        public int StatusCode { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
