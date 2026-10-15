using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Contracts;
using static RaceDay.Contracts.Dtos;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")] // Placed explicitly matching guide URL requirement
public class EnrollmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrollmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // 3. POST /api/enrolments -> Participant only
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public IActionResult CreateEnrollment([FromBody] CreateEnrolmentRequest request)
    {
        return Ok(new { Message = "Success! Only logged-in Participants can enroll in races." });
    }
}
