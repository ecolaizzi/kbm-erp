namespace KBM.Application.Accounting;

public record ChartAccountListItem(
    long Id,
    long? ParentId,
    int Level,
    string Code,
    string FullCode,
    string Name,
    int Nature,
    int Sign,
    int SubKind,
    bool AllowsPosting,
    string Status);

public record ChartAccountDetail(
    long Id,
    long? ParentId,
    int Level,
    string Code,
    string FullCode,
    string Name,
    int Nature,
    int Sign,
    int SubKind,
    bool AllowsPosting,
    string? BilCeeDare,
    string? BilCeeAvere,
    string Status);

public record CreateChartAccountRequest(
    long? ParentId,
    string Code,
    string Name,
    int? Nature,
    int Sign,
    int SubKind,
    string? BilCeeDare,
    string? BilCeeAvere);

public record UpdateChartAccountRequest(
    string Name,
    int Sign,
    int SubKind,
    string? BilCeeDare,
    string? BilCeeAvere,
    string Status);
