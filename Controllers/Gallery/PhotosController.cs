using Microsoft.AspNetCore.Identity;

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
                int order = -1)
        {
            // Base query
            IQueryable<ViPhoto> query = context.ViPhotos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Title, $"%{q}%") ||
                    EF.Functions.Like(p.Description ?? string.Empty, $"%{q}%") ||
                    EF.Functions.Like(p.OwnerName ?? string.Empty, $"%{q}%"));
            }

            //  Sorting 
            query = sortField switch
            {
                "Title" or "title" => order == 1 ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title),
                "OwnerName" or "ownername" => order == 1 ? query.OrderBy(p => p.OwnerName) : query.OrderByDescending(p => p.OwnerName),
                _ => order == 1 ? query.OrderBy(p => p.UploadedDate) : query.OrderByDescending(p => p.UploadedDate)
            };

            //  Count total records
            var recordsTotal = await query.CountAsync();

            //  Data fetch 
            var records = await query
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
                    Tags = string.IsNullOrEmpty(p.Tagging)
                        ? Array.Empty<string>()
                        : p.Tagging.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    Thumbnail = $"data:{GetImageMimeType(p.ImageData)};base64,{Convert.ToBase64String(p.ImageData)}"
                })
                .ToListAsync();

            return ResponseHelper.OK_Result(
                new { records, recordsTotal },
                new DefaultResponseMessageModel("Successfully retrieved photos.", "အောင်မြင်ပါသည်။"));
        }

        [HttpGet("tag")]
        [EndpointSummary("Get Tags")]
        public async Task<IActionResult> GetTagsAsync()
        {
            return ResponseHelper.OK_Result(await context.Tags.ToListAsync(), null);
        }

        [HttpGet("{id}")]
        [EndpointSummary("Get By Id.")]
        public async Task<IActionResult> GetPhotoDetail(int id)
        {
            ViPhoto? photo = await context.ViPhotos
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
            }, new DefaultResponseMessageModel("Successfully Get Data.", "အောင်မြင်ပါသည်။"));
        }

        [HttpPost("upload")]
        [EndpointSummary("Photo Upload")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoModel model, CancellationToken cancellationToken)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest("No file uploaded.");

            var user = await userManager.GetUserAsync(User);

            await using var ms = new MemoryStream();
            await model.File.CopyToAsync(ms, cancellationToken);
            var imageBytes = ms.ToArray();

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var photo = new Photo
                {
                    Title = model.Title?.Trim(),
                    Description = model.Description?.Trim(),
                    Location = model.Location?.Trim(),
                    OwnerId = user?.Id ?? string.Empty,
                    ImageData = imageBytes,
                    Tagging = model.Tags?.Trim(),
                    UploadedDate = DateTime.Now
                };

                await context.Photos.AddAsync(photo, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(model.Tags))
                {
                    var tagNames = model.Tags
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    // Get all existing tags
                    var existingTags = await context.Tags
                        .Where(t => tagNames.Contains(t.Name))
                        .ToListAsync(cancellationToken);

                    var newTagNames = tagNames
                        .Except(existingTags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    if (newTagNames.Length > 0)
                    {
                        var newTags = newTagNames.Select(name => new Tag { Name = name }).ToList();
                        await context.Tags.AddRangeAsync(newTags, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                        existingTags.AddRange(newTags);
                    }

                    // Add mapping PhotoTags
                    var photoTags = existingTags.Select(t => new PhotoTag
                    {
                        PhotoId = photo.Id,
                        TagId = t.Id
                    });

                    await context.PhotoTags.AddRangeAsync(photoTags, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // Commit all if successful
                await transaction.CommitAsync(cancellationToken);

                return ResponseHelper.OK_Result(photo.Id,
                    new DefaultResponseMessageModel("Successfully uploaded photo.", "အောင်မြင်စွာတင်ခဲ့ပါသည်။"));
            }
            catch (Exception ex)
            {
                //  Rollback everything if any failure have
                await transaction.RollbackAsync(cancellationToken);

                return ResponseHelper.InternalServerError_Request(null, new DefaultResponseMessageModel($"Upload failed: {ex.Message}",""));
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        [EndpointSummary("Delete Photo")]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            IdentityUser? user = await userManager.GetUserAsync(User);
            Photo? photo = await context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null) return NotFound("Photo not found");

            bool isAdmin = await userManager.IsInRoleAsync(user!, "admin");
            if (!isAdmin)
                return Forbid("You are not authorized to delete this photo");

            var photoTags = await context.PhotoTags.Where(pt => pt.PhotoId == id).ToListAsync();
            if (photoTags.Any())
            {
                context.PhotoTags.RemoveRange(photoTags);
            }

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
