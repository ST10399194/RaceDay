using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Participant
{
    [Key]
    public int ParticipantID { get; set; }

    public int UserID { get; set; }
    [ForeignKey("UserID")]
    public User? User { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    // Navigation Properties (1-to-many)
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}