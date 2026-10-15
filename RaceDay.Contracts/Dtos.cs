using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts
{
    public class Dtos
    {
        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterRequest
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        public class AuthResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        // --- PROFILE DTOs ---

        public class ProfileDto
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class UpdateProfileRequest
        {
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
        }

        // --- EVENT DTOs ---

        public class CreateEventRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }

        public class UpdateEventRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }

        public class EventDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public DateTime Date { get; set; }
        }

        // --- CATEGORY DTOs ---

        public class CreateCategoryRequest
        {
            public string Name { get; set; } = string.Empty;
        }

        public class CategoryDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        // --- ENROLMENT DTOs ---

        public class CreateEnrolmentRequest
        {
            public int EventId { get; set; }
            public int CategoryId { get; set; }
        }

        public class UpdateEnrolmentStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        public class EnrolmentDto
        {
            public int Id { get; set; }
            public int EventId { get; set; }
            public string EventName { get; set; } = string.Empty;
            public string UserEmail { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        // --- RESULT DTOs ---

        public class CreateResultRequest
        {
            public int EnrolmentId { get; set; }
            public string TimeTaken { get; set; } = string.Empty;
            public int Position { get; set; }
        }

        public class ResultDto
        {
            public int Id { get; set; }
            public int EnrolmentId { get; set; }
            public string RunnerName { get; set; } = string.Empty;
            public string TimeTaken { get; set; } = string.Empty;
            public int Position { get; set; }
        }

        // --- IMAGE UPLOAD DTO (Part 3) ---

        public class UploadResponse
        {
            public string Url { get; set; } = string.Empty;
            public bool IsSuccess { get; set; }
        }
    }
}