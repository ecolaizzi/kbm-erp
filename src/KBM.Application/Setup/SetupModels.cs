namespace KBM.Application.Setup;

public record SetupStatusResponse(bool Required, bool Completed, string? Message);

public record SetupCompleteRequest(
    string CompanyCode,
    string BusinessName,
    string? VatNumber,
    string AdminUsername,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName);

public record SetupCompleteResponse(long CompanyId, long UserId, string Message);
