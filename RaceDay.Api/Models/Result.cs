using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        public int EnrollmentID { get; set; }
        [ForeignKey("EnrollmentID")]
        public Enrollment? Enrollment { get; set; }

        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}
