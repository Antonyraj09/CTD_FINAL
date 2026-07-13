namespace CTD_FINAL.DTOs;

// All figures below are derived from JobIsne — it has no WorkflowStatus/BillingStatus/
// BorderPoint FK, so "status" here is the 3-state pseudo-status (Helpers/JobIsneStatus.cs),
// "route" is the free-text RouteOfTransit field, and duty/commercial figures come from
// DutyAmount/FobValue/CifInr rather than an invoice/billing total (JobIsne has none).

public class DashboardKpiDto
{
    public int Total { get; set; }
    public int CtdIssued { get; set; }
    public int Arrived { get; set; }
    public int PendingCtd { get; set; }
    public int GreenCtdCount { get; set; }
    public decimal TotalDuty { get; set; }
}

public class MonthlyPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalDuty { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RouteCountDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardAlertDto
{
    public string Type { get; set; } = "info"; // warning | error | info
    public string Title { get; set; } = string.Empty;
    public string Sub { get; set; } = string.Empty;
}

public class RecentJobDto
{
    public int Id { get; set; }
    public string JobNo { get; set; } = string.Empty;
    public DateTime JobDate { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string? CtdNumber { get; set; }
    public int ContainerCount { get; set; }
    public string? ContainerSize { get; set; }
    public string? Route { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CustomerKpiDto
{
    public int TotalShipments { get; set; }
    public int CtdIssued { get; set; }
    public int Arrived { get; set; }
    public int PendingCtd { get; set; }
}

public class CustomerShipmentDto
{
    public int Id { get; set; }
    public string JobNo { get; set; } = string.Empty;
    public string? CtdNumber { get; set; }
    public string? Container { get; set; }
    public string? Route { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ArrivalDate { get; set; }
}

public class CustomerCommercialDto
{
    public decimal TotalFobValue { get; set; }
    public decimal TotalCifInr { get; set; }
    public decimal TotalDuty { get; set; }
}

public class TimelineStepDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string CssClass { get; set; } = string.Empty; // done | current | ""
}
