using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhotoGallery.Pages.Photos
{
    [Authorize]
    public class ListModel : PageModel
    {
        public List<PhotoItem> PhotoList { get; set; } = [];
        public void OnGet()
        {
            
        }

        public record PhotoItem(string Url, string Title);
    }
}
