using System;
using System.Collections.Generic;

namespace RaceDay.Mvc.ViewModels;

public class OrganiserDashboardVm
{
    public List<OrganiserEventSummaryVm> Events { get; set; } = new();
}

public class OrganiserEventSummaryVm
{
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int EnrolmentCount { get; set; }
}

public class CaptureResultVm
{
    public int EnrolmentId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int FinishingPosition { get; set; }
}