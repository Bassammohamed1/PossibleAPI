using GP_API.Helpers;

namespace GP_API.DTOs
{
    public class ChildDTO
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Difficult { get; set; }
        public Gender Gender { get; set; }
        public IFormFile ClientFile { get; set; }
        public string ParentUserName { get; set; }
    }
}
