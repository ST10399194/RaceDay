namespace RaceDay.Api.Tests;

public static class ApiRoutes
{
    public const string Register = "/api/auth/register";
    public const string Login = "/api/auth/login";
    public const string Logout = "/api/auth/logout";

    public const string Profile = "/api/profile/me";

    public const string Events = "/api/events";
    public const string MyEvents = "/api/events/mine";
    public static string Event(int id) => $"/api/events/{id}";

    public static string EventCategories(int eventId) => $"/api/events/{eventId}/categories";
    public static string Category(int id) => $"/api/categories/{id}";

    public const string Enrolments = "/api/enrolments";
    public const string MyEnrolments = "/api/enrolments/mine";
    public static string EventEnrolments(int eventId) => $"/api/enrolments/event/{eventId}";
    public static string EnrolmentStatus(int id) => $"/api/enrolments/{id}/status";

    public const string Results = "/api/results";
    public const string MyResults = "/api/results/mine";
    public static string EventResults(int eventId) => $"/api/results/event/{eventId}";
}