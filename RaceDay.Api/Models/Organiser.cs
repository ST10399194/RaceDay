using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Organiser
    {
        [Key]
        public int OrganiserID { get; set; }

        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }

        public string OrganisationName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;

        // Navigation Properties (1-to-many)
        public ICollection<Event> Events { get; set; } = new List<Event>();
    
}
}
