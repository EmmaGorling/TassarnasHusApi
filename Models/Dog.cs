using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TassarnasHusApi.Models;

public class DogModel {
    public int Id { get; set; }
    [Required]
    [Display(Name = "Namn")]
    public string? Name { get; set; }
    [Required]
    [Display(Name = "Ras")]
    public string? Breed { get; set; }
    [Required]
    [Display(Name = "Stad")]
    public string? City { get; set; }
    [Required]
    [Display(Name = "Kön")]
    public string? Sex { get; set; }
    [Required]
    [Display(Name = "Beskrivning")]
    public string? Description { get; set; }
    [Required]
    [Display(Name = "Ålder")]
    public int Age { get; set; }
    [Required]
    [Display(Name = "Storlek")]
    public string? Size { get; set; }
    [Required]
    [Display(Name = "Adopterad")]
    public bool Adopted { get; set; } = false;
    [Required]
    [Display(Name = "Tillagd")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ImageName { get; set; }
    [NotMapped]
    [Display(Name = "Bild")]
    public IFormFile? ImageFile { get; set; }
    public string? CreatedBy { get; set; }
}