namespace CTD_FINAL.Enums;

public enum WorkflowStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Transit = 3,
    Delivered = 4,
    Closed = 5
}

public enum ShipmentType
{
    Single = 0,
    Multiple = 1
}

public enum CtdType
{
    EDI = 0,
    Manual = 1
}

public enum BillingStatus
{
    Unpaid = 0,
    Partial = 1,
    Paid = 2
}

public enum DeliveryStatusType
{
    InTransit = 0,
    ArrivedAtBorder = 1,
    CustomsClearedNepal = 2,
    Delivered = 3,
    Delayed = 4
}

public enum AuditActionType
{
    Created = 0,
    Updated = 1,
    StatusChange = 2,
    DocumentGenerated = 3,
    Deleted = 4
}

public enum AlertChannel
{
    Email = 0,
    Sms = 1,
    EmailAndSms = 2
}

public enum ContainerStatus
{
    FCL = 0,
    LCL = 1
}
