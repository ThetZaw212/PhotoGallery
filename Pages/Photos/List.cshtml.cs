using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhotoGallery.Pages.Photos
{
    [Authorize]
    public class ListModel : PageModel
    {
        public List<PhotoItem> PhotoList { get; set; } = [];
        public void OnGet()
        {
            PhotoList =
            [
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
                new PhotoItem("https://picsum.photos/id/1015/400/300", "Mountain View"),
                new PhotoItem("https://picsum.photos/id/1025/400/300", "Beach Sunset"),
                new PhotoItem("https://picsum.photos/id/1035/400/300", "City Lights"),
                new PhotoItem("https://picsum.photos/id/1045/400/300", "Forest Trail"),
            ];
        }

        public record PhotoItem(string Url, string Title);
    }
}
