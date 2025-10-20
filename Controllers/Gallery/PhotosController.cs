namespace PhotoGallery.Controllers.Gallery
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController(PhotoGalleryDbContext context, UserManager<IdentityUser> userManager) : ControllerBase
    {
        [HttpGet]
        [EndpointSummary("Get Photos with Pagination")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPhotosAsync(
                     int skipRows = 0,
                     int pageSize = 10,
                     string? q = null,
                     string? sortField = "UploadedDate",
                     int order = -1
                 )
        {
            IQueryable<ViPhoto> photosQuery = context.ViPhotos.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                photosQuery = photosQuery.Where(p =>
                    p.Title.Contains(q) ||
                    (p.Description ?? string.Empty).Contains(q) ||
                    (p.OwnerName ?? string.Empty).Contains(q));
            }

            photosQuery = sortField?.ToLower() switch
            {
                "title" => order == 1 ? photosQuery.OrderBy(p => p.Title) : photosQuery.OrderByDescending(p => p.Title),
                "ownername" => order == 1 ? photosQuery.OrderBy(p => p.OwnerName) : photosQuery.OrderByDescending(p => p.OwnerName),
                _ => order == 1 ? photosQuery.OrderBy(p => p.UploadedDate) : photosQuery.OrderByDescending(p => p.UploadedDate),
            };

            int recordsTotal = await photosQuery.CountAsync();

            var records = await photosQuery
                .Skip(skipRows)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.Location,
                    p.OwnerName,
                    UploadedDate = p.UploadedDate.ToString("yyyy-MM-dd"),
                    Tags = p.Tagging != null
                        ? p.Tagging.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        : Array.Empty<string>(),
                    Thumbnail = $"data:{GetImageMimeType(p.ImageData)};base64,{Convert.ToBase64String(p.ImageData)}"
                })
                .ToListAsync();

            return ResponseHelper.OK_Result(new { records, recordsTotal },
                new DefaultResponseMessageModel("Successfully retrieved photos.", "အောင်မြင်ပါသည်။"));
        }

        [HttpGet("tag")]
        [EndpointSummary("Get Tags")]
        public async Task<IActionResult> GetTagsAsync() 
        {
            List<Tag> tag = await context.Tags.ToListAsync();

            return ResponseHelper.OK_Result(tag, null);
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get By Id.")]
        public async Task<IActionResult> GetPhotoDetail(int id)
        {
            var photo = await context.ViPhotos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null) return NotFound("Photo not found");

            return ResponseHelper.OK_Result(new
            {
                photo.Id,
                photo.Title,
                photo.Description,
                photo.Location,
                photo.OwnerName,
                UploadedDate = photo.UploadedDate.ToString("yyyy-MM-dd"),
                Tags = photo.Tagging != null ? photo.Tagging.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : [],
                Thumbnail = $"data:{GetImageMimeType(photo.ImageData)};base64,{Convert.ToBase64String(photo.ImageData)}"
            },new DefaultResponseMessageModel("Successfully Get Data.", "အောင်မြင်ပါသည်။"));
        }

        [Authorize]
        [HttpPost("upload")]
        [EndpointSummary("Photo Upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoModel model)
        {
            // Get user id manually from the JWT
            var userId = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token is missing or invalid");

            // Then find the user manually
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized("User not found");

            if (model.File == null || model.File.Length == 0)
                return BadRequest("No file uploaded");

            using var ms = new MemoryStream();
            await model.File.CopyToAsync(ms);

            var photo = new Photo
            {
                Title = model.Title,
                Description = model.Description,
                Location = model.Location,
                OwnerId = user.Id,
                ImageData = ms.ToArray(),
                UploadedDate = DateTime.Now
            };
            context.Photos.Add(photo);
            await context.SaveChangesAsync();
                
            // Handle tags (comma-separated)
            if (!string.IsNullOrWhiteSpace(model.Tags))
            {
                var tagNames = model.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var photoTagMappings = new List<PhotoTag>();
                foreach (var name in tagNames)
                {
                    var tag = await context.Tags.FirstOrDefaultAsync(t => t.Name == name)
                              ?? (await context.Tags.AddAsync(new Tag { Name = name })).Entity;

                    photoTagMappings.Add(new PhotoTag { PhotoId = photo.Id, TagId = tag.Id });
                }
                context.PhotoTags.AddRange(photoTagMappings);
                await context.SaveChangesAsync();
            }

            return ResponseHelper.OK_Result(photo.Id, new DefaultResponseMessageModel("Successfull Upload","Fail To Upload"));
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EndpointSummary("Delete Photo")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var photo = await context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null) return NotFound("Photo not found");

            var isAdmin = await userManager.IsInRoleAsync(user!, "admin");
            if (photo.OwnerId != user!.Id && !isAdmin)
                return Forbid("You are not authorized to delete this photo");

            context.Photos.Remove(photo);
            await context.SaveChangesAsync();

            return ResponseHelper.OK_Result(null, new DefaultResponseMessageModel("Photo deleted successfully", "Photo deleted successfully"));
        }

        [NonAction]
        private static string GetImageMimeType(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
                return "image/jpeg"; 

            // JPEG
            if (bytes[0] == 0xFF && bytes[1] == 0xD8)
                return "image/jpeg";

            // PNG
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            // Default
            return "image/jpeg";
        }


    }
}
