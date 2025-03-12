
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TassarnasHusApi.Models;

public class News {
    public int Id { get; set; }
    [Required]
    [Display(Name = "Titel")]
    public string? Title { get; set; }
    [Required]
    [Display(Name = "Innehåll")]
    public string? Content { get; set; }
    [Required]
    [Display(Name = "Tillagd")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ImageName { get; set; }
    [NotMapped]
    [Display(Name = "Bild")]
    public IFormFile? ImageFile { get; set; }
    public string? CreatedBy { get; set; }
}