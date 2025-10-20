using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhotoGallery.Entities;

[Index("Name", Name = "UQ__Tags__737584F66F6C8843", IsUnique = true)]
public partial class Tag
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    [InverseProperty("Tag")]
    public virtual ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
}
