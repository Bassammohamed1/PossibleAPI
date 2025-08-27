namespace GP_API.Models
{
    public class DbOperationModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public Child? Child { get; set; }
    }
}
