using Mapster;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.AccessLogs;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.FileServices;

namespace Opti_Sec_Backend.Services.AccessLogServices;

public class AccessLogService(
    ApplicationDbContext context,
    IFileService fileService,
    IHttpContextAccessor httpContextAccessor) : IAccessLogService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IFileService _fileService = fileService;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;


    public async Task<Result> CheckOrCreateAsync(
    AccessLogRequest request,
    CancellationToken cancellationToken)
    {
        
        var gate = await _context.Gates
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x =>
                x.Id == request.GateId &&
                !x.IsDeleted,
            cancellationToken);

        if (gate is null)
            return Result.Failure(GateErrors.NotFound);

        // Validate request data
        if (
            string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.FingerprintTemplate))
        {
            return Result.Failure(AccessLogErrors.InvalidCredentials);
        }

        // Search member using Username + Fingerprint
        var member = await _context.Members
            .FirstOrDefaultAsync(x =>
                x.UserName == request.UserName &&
                x.FingerprintTemplate == request.FingerprintTemplate &&
                x.ClientId == gate.ClientId &&
                !x.IsDeleted,
            cancellationToken);

       
        // AUTHORIZED ACCESS
        if (member is not null)
        {
            await _context.AccessLogs.AddAsync(new AccessLog
            {
                GateId = request.GateId,
                MemberId = member.Id,
                IsAuthorized = true,
                CreatedById = gate.Client.UserId,
                ImageUrl = member.ImageUrl
            }, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        // UNAUTHORIZED ACCESS
        string? imageName = null;

        if (request.Image is not null)
        {
            imageName = await _fileService.UploadImageAsync(
                request.Image,
                cancellationToken);
        }

        await _context.AccessLogs.AddAsync(new AccessLog
        {
            GateId = request.GateId,
            MemberId = null,
            IsAuthorized = false,
            CreatedById = gate.Client.UserId,
            ImageUrl = imageName
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Failure(AccessLogErrors.MemberNotBelong);
    }

    public async Task<Result<IEnumerable<AccessLogResponse>>> GetAuthorizedAccessLogAsync(string clientId,CancellationToken cancellationToken)
    {
        //var authorizedAccessLog = await _context.AccessLogs
        //    .Include(x => x.Member)
        //    .Include(x => x.Gate)
        //    .ThenInclude(x => x.Client)
        //    .Where(x => x.Gate.Client.UserId == clientId && !x.IsDeleted && x.IsAuthorized)
        //    .Select(x => new AccessLogResponse
        //    (
        //        x.Id,
        //        x.Member != null
        //            ? $"{x.Member.FName} {x.Member.LName}"
        //            : "Unknown",
        //        x.ImageUrl!,
        //        DateOnly.FromDateTime(x.DateTime),
        //        TimeOnly.FromDateTime(x.DateTime)
        //    ))
        //    .ToListAsync(cancellationToken);

        var authorizedAccessLog = await _context.AccessLogs
        .AsNoTracking()
        .Where(x =>
            x.Gate!.Client.UserId == clientId &&
            !x.IsDeleted &&
            x.IsAuthorized)
        .OrderByDescending(x => x.DateTime)
        .Select(x => new
        {
            Id = x.Id,
            Member = x.Member != null
                ? x.Member.FName + " " + x.Member.LName
                : "Unknown",
            Gate = x.Gate!.Name,
            ImageUrl = x.ImageUrl,
            Date = DateOnly.FromDateTime(x.DateTime),
            Time = TimeOnly.FromDateTime(x.DateTime)
        })
        .ToListAsync(cancellationToken);

        var authorizedAccessLogResponse = authorizedAccessLog.Select(x => new AccessLogResponse
        (
            x.Id,
            x.Member,
            x.Gate,
            BuildImageUrl(x.ImageUrl),
            x.Date,
            x.Time
        ));

        return Result.Success<IEnumerable<AccessLogResponse>>(authorizedAccessLogResponse);
    }

    public async Task<Result<IEnumerable<AccessLogResponse>>> GetUnAuthorizedAccessLogAsync(string clientId,CancellationToken cancellationToken)
    {
        var unAuthorizedAccessLog = await _context.AccessLogs
        .AsNoTracking()
        .Where(x =>
            x.Gate!.Client.UserId == clientId &&
            !x.IsDeleted &&
            !x.IsAuthorized)
        .OrderByDescending(x => x.DateTime)
        .Select(x => new
        {
            Id = x.Id,
            Member = x.Member != null
                ? x.Member.FName + " " + x.Member.LName
                : "Unknown",
            Gate = x.Gate!.Name,
            ImageUrl = x.ImageUrl,
            Date = DateOnly.FromDateTime(x.DateTime),
            Time = TimeOnly.FromDateTime(x.DateTime)
        })
        .ToListAsync(cancellationToken);

        var unAuthorizedAccessLogResponse = unAuthorizedAccessLog.Select(x => new AccessLogResponse
        (
            x.Id,
            x.Member,
            x.Gate,
            BuildImageUrl(x.ImageUrl),
            x.Date,
            x.Time
        ));

        return Result.Success<IEnumerable<AccessLogResponse>>(unAuthorizedAccessLogResponse);
    }

    private string BuildImageUrl(string? imageName)
    {
        if (string.IsNullOrEmpty(imageName))
            return $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext!.Request.Host}/Images/default.png";

        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        return $"{baseUrl}/Images/{imageName}";
    }
}
