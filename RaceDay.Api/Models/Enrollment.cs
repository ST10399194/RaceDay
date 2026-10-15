using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        public int ParticipantID { get; set; }
        [ForeignKey("ParticipantID")]
        public Participant? Participant { get; set; }

        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public Event? Event { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";

        // Navigation Properties (1-to-0..1 relationship)
        public Result? Result { get; set; }
    }
}
