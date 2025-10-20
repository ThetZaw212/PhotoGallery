namespace PhotoGallery.Models
{
    public class UploadPhotoModel
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Tags { get; set; } = null!;
        public IFormFile File { get; set; } = null!;
    }
}
