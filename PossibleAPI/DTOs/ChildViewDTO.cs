using GP_API.Helpers;

namespace GP_API.DTOs
{
    public class ChildViewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Gender Gender { get; set; }
        public int ReadingRate { get; set; }
        public int WritingRate { get; set; }
        public string LastReadingTime { get; set; }
        public string LastWritingTime { get; set; }
        public int ReadingDays { get; set; }
        public int WritingDays { get; set; }
        public string Difficult { get; set; }
        public string ParentUserName { get; set; }
        public string Image { get; set; }
    }
}
