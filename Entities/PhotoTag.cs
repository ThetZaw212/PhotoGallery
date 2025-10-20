using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhotoGallery.Entities;

[PrimaryKey("PhotoId", "TagId")]
public partial class PhotoTag
{
    [Key]
    public int PhotoId { get; set; }

    [Key]
    public int TagId { get; set; }

    [ForeignKey("TagId")]
    [InverseProperty("PhotoTags")]
    public virtual Tag Tag { get; set; } = null!;
}
