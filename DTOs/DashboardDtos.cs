namespace CTD_FINAL.DTOs;

public class DashboardKpiDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Delivered { get; set; }
    public int PendingCtd { get; set; }
    public int PendingBilling { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Revenue { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class BorderPointCountDto
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
    public string ImporterName { get; set; } = string.Empty;
    public string? CtdNumber { get; set; }
    public int ContainerCount { get; set; }
    public string? ContainerSize { get; set; }
    public string? BorderPoint { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BillingStatus { get; set; } = string.Empty;
}

public class CustomerKpiDto
{
    public int TotalShipments { get; set; }
    public int InTransit { get; set; }
    public int Delivered { get; set; }
    public int OutstandingInvoices { get; set; }
}

public class CustomerShipmentDto
{
    public int Id { get; set; }
    public string JobNo { get; set; } = string.Empty;
    public string? CtdNumber { get; set; }
    public string Containers { get; set; } = string.Empty;
    public string? BorderPoint { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpDeliveryDate { get; set; }
}

public class CustomerBillingDto
{
    public decimal TotalBilled { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Outstanding { get; set; }
}

public class TimelineStepDto
{
    public string Label { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string CssClass { get; set; } = string.Empty; // done | current | ""
}
