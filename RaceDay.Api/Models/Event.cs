using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        public int OrganiserID { get; set; }
        [ForeignKey("OrganiserID")]
        public Organiser? Organiser { get; set; }

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category? Category { get; set; }

        [Required]
        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string RouteInformation { get; set; } = string.Empty;
        public string WeatherInformation { get; set; } = string.Empty;

        // Navigation Properties (1-to-many)
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
