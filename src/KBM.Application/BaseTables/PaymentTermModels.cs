namespace KBM.Application.BaseTables;

public record PaymentTermListItem(
    long Id,
    string Code,
    string Description,
    int InstallmentsCount,
    int FirstDueDays,
    int IntervalDays,
    bool EndOfMonth,
    string? PaymentMethod,
    string Status);

public record PaymentTermDetail(
    long Id,
    string Code,
    string Description,
    int InstallmentsCount,
    int FirstDueDays,
    int IntervalDays,
    bool EndOfMonth,
    string? PaymentMethod,
    string? Notes,
    string Status);

public record CreatePaymentTermRequest(
    string Code,
    string Description,
    int InstallmentsCount,
    int FirstDueDays,
    int IntervalDays,
    bool EndOfMonth,
    string? PaymentMethod,
    string? Notes);

public record UpdatePaymentTermRequest(
    string Description,
    int InstallmentsCount,
    int FirstDueDays,
    int IntervalDays,
    bool EndOfMonth,
    string? PaymentMethod,
    string? Notes,
    string Status);
