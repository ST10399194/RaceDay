using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public double DistanceKM { get; set; }

        // Navigation Properties (1-to-many)
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}