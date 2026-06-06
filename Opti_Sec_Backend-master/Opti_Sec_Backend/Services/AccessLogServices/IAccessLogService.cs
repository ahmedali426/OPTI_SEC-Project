using Opti_Sec_Backend.Contracts.AccessLogs;

namespace Opti_Sec_Backend.Services.AccessLogServices;

public interface IAccessLogService 
{
    public Task<Result> CheckOrCreateAsync(AccessLogRequest request, CancellationToken cancellationToken);
    public Task<Result<IEnumerable<AccessLogResponse>>> GetAuthorizedAccessLogAsync(string clientId,CancellationToken cancellationToken);
    public Task<Result<IEnumerable<AccessLogResponse>>> GetUnAuthorizedAccessLogAsync(string clientId, CancellationToken cancellationToken);
}
