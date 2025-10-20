using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PhotoGallery.Pages.Photos
{
    [Authorize]
    public class UploadModel(PhotoGalleryDbContext context) : PageModel
    {
        private readonly PhotoGalleryDbContext context;


        [BindProperty]
        public Photo Photo { get; set; } = null!;

        [BindProperty]
        public IFormFile UploadFile { get; set; } = null!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (UploadFile != null)
            {
                using var ms = new MemoryStream();
                await UploadFile.CopyToAsync(ms);
                Photo.ImageData = ms.ToArray();
            }

            Photo.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Photo.UploadedDate = DateTime.Now;

            context.Photos.Add(Photo);
            await context.SaveChangesAsync();

            return RedirectToPage("/Photos/List");
        }
    }
}
