using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TassarnasHusApi.Models;

public class Application {
    public int Id { get; set; }
    [Required]
    [Display(Name = "Namn")]
    public string? Name { get; set; }
    [Required]
    [Display(Name = "E-post")]
    public string? Email { get; set; }
    [Required]
    [Display(Name = "Meddelande")]
    public string? Message { get; set; }
    [Required]
    [Display(Name = "Hanterad")]
    public bool Handeled { get; set; } = false;
    [Required]
    [Display(Name = "Tillagd")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int DogId { get; set; }
    [ForeignKey("DogId")]
    public Dog? Dog { get; set; }
}