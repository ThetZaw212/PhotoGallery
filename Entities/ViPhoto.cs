using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhotoGallery.Entities;

[Keyless]
public partial class ViPhoto
{
    public int Id { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(450)]
    public string OwnerId { get; set; } = null!;

    [StringLength(256)]
    public string? OwnerName { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UploadedDate { get; set; }

    [StringLength(100)]
    public string? Tagging { get; set; }

    public byte[] ImageData { get; set; } = null!;
}
