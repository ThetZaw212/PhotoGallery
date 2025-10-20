using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhotoGallery.Pages.Photos
{
    [Authorize]
    public class UploadModel() : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            
        }
    }
}
