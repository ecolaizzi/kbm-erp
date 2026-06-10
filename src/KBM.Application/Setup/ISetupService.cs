namespace KBM.Application.Setup;

public interface ISetupService
{
    Task<SetupStatusResponse> GetStatusAsync(CancellationToken ct = default);
    Task<SetupCompleteResponse> CompleteAsync(SetupCompleteRequest request, CancellationToken ct = default);
    Task MarkCompletedIfLegacyDataAsync(CancellationToken ct = default);
}
