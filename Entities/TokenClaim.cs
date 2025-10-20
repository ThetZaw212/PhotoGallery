using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PhotoGallery.Entities;

[Table("TokenClaim")]
public partial class TokenClaim
{
    [Key]
    public string UserId { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? RefreshDate { get; set; }

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? TokenExpiry { get; set; }
}
