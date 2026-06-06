using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.Members;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.AIServices;
using Opti_Sec_Backend.Services.FileServices;

namespace Opti_Sec_Backend.Services.MemberServices;

public class MemberService(
    ApplicationDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IFileService fileService,
    IAITrainingService aiTrainingService) : IMemberService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IFileService _fileService = fileService;
    private readonly IAITrainingService _aiTrainingService = aiTrainingService;

    public async Task<Result<MemberResponse>> CreateAsync(string clientId,MemberRequest request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => (x.UserId == clientId) && !x.IsDeleted,cancellationToken);

        if (client is null)
            return Result.Failure<MemberResponse>(ClientErrors.NotFound);

        var memberUsernameExist = await _context.Members
            .AnyAsync(x => x.UserName == request.UserName, cancellationToken);

        if (memberUsernameExist)
            return Result.Failure<MemberResponse>(MemberError.UserNameExist);

        string? imageName = null;

        if (request.Image is not null)
        {
            imageName = await _fileService.UploadImageAsync(request.Image, cancellationToken);
        }

        var member = new Member
        {
            FName = request.FName,
            LName = request.LName,
            UserName = request.UserName,
            Phone = request.Phone,
            ImageUrl = imageName!,
            ClientId = client.Id,
            AITrainingStatus = AITrainingStatus.Pending
        };

        await _context.Members.AddAsync(member, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);


        var response = new MemberResponse(
            member.Id,
            $"{member.FName} {member.LName}",
            member.UserName,
            member.Phone,
            BuildImageUrl(member.ImageUrl)
        );

        // Trigger proactive AI model training in background
        await _aiTrainingService.TriggerTrainingAsync(member.Id, response.ImageUrl, cancellationToken);

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<GetAllMemberResponse>>> GetAllAsync(
    string? search,
    string userId,
    CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => (x.UserId == userId) && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure<IEnumerable<GetAllMemberResponse>>(ClientErrors.NotFound);

        var query = _context.Members
            .Where(x => x.ClientId == client.Id && !x.IsDeleted)
            .AsQueryable();

        
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(x.FName.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.LName.ToLower(), $"%{search}%")
            );
        }

        var members = await query
            .Select(x => new
            {
                x.Id,
                Name = x.FName + " " + x.LName,
                x.ImageUrl
            })
            .ToListAsync(cancellationToken);

        var response = members.Select(x => new GetAllMemberResponse(
            x.Id,
            x.Name,
            BuildImageUrl(x.ImageUrl)
        ));

        return Result.Success<IEnumerable<GetAllMemberResponse>>(response);
    }

    public async Task<Result<MemberResponse>> GetByIdAsync(string userId,int id, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => (x.UserId == userId) && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure<MemberResponse>(ClientErrors.NotFound);

        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.ClientId == client.Id, cancellationToken);

        if (member is null)
            return Result.Failure<MemberResponse>(MemberError.NotFound);

        var response = new MemberResponse(
            member.Id,
            $"{member.FName} {member.LName}",
            member.UserName,
            member.Phone,
            BuildImageUrl(member.ImageUrl)
        );

        return Result.Success(response);
    }

    public async Task<Result> UpdateAsync(string userId,int id, MemberUpdateRequest request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => (x.UserId == userId) && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure(ClientErrors.NotFound);


        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.ClientId == client.Id, cancellationToken);

        if (member is null)
            return Result.Failure(MemberError.NotFound);

        var memberUsernameExist = await _context.Members
            .AnyAsync(x => x.UserName == request.UserName && x.Id != id,cancellationToken);

        if(memberUsernameExist) 
            return Result.Failure(MemberError.UserNameExist);

        bool imageUpdated = false;
        if (request.Image is not null)
        {
            member.ImageUrl = await _fileService.UpdateImageAsync(member.ImageUrl, request.Image, cancellationToken);
            member.AITrainingStatus = AITrainingStatus.Pending;
            imageUpdated = true;
        }

        var (firstName, lastName) = SplitName(request.Name);

        member.FName = firstName;
        member.LName = lastName;

        member.UserName = request.UserName;
        member.Phone = request.Phone;

        await _context.SaveChangesAsync(cancellationToken);

        if (imageUpdated)
        {
            var imageUrl = BuildImageUrl(member.ImageUrl);
            await _aiTrainingService.TriggerTrainingAsync(member.Id, imageUrl, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => (x.UserId == userId) && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure(ClientErrors.NotFound);

        var member = await _context.Members
            .FirstOrDefaultAsync(x => x.Id == id && x.ClientId == client.Id, cancellationToken);

        if (member is null)
            return Result.Failure(MemberError.NotFound);

        member.IsDeleted = true;

        if (!string.IsNullOrEmpty(member.ImageUrl))
        {
            await _fileService.DeleteImageAsync(member.ImageUrl);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<int>> CountAsync(string userId, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(x => (x.UserId == userId) && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure<int>(ClientErrors.NotFound);

        var membersCount = await _context.Members.Where(x => !x.IsDeleted && x.ClientId == client.Id).CountAsync(cancellationToken);

        return Result.Success(membersCount);
    }

    public async Task<Result<IEnumerable<GetAllMemberForAI>>> GetAllForAIAsync(CancellationToken cancellationToken)
    {
        var members = await _context.Members
        .Where(x => !x.IsDeleted)
        .Select(x => new
        {
            x.Id,
            x.UserName,
            Name = x.FName + " " + x.LName,
            x.ImageUrl,
            x.IsDeleted
        })
        .ToListAsync(cancellationToken);

            var response = members.Select(x => new GetAllMemberForAI(
                x.Id,
                x.Name,
                x.UserName,
                BuildImageUrl(x.ImageUrl),
                x.IsDeleted
            ));

        return Result.Success<IEnumerable<GetAllMemberForAI>>(response);
    }

    public async Task<Result> SetFingerprintAsync(SetFingerPrintRequest request, CancellationToken cancellationToken)
    {
        var member = await _context.Members
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == request.MemberId && !x.IsDeleted, cancellationToken);

        if (member is null)
        {
            return Result.Failure(MemberError.NotFound);
        }

        // Check if fingerprint already exists for another member
        var fingerprintExists = await _context.Members
            .AnyAsync(x =>
                x.Id != request.MemberId &&
                !x.IsDeleted &&
                x.FingerprintTemplate == request.FingerprintTemplate,
                cancellationToken);

        if (fingerprintExists)
        {
            return Result.Failure(MemberError.FingerprintAlreadyExists);
        }

        member.FingerprintTemplate = request.FingerprintTemplate;

        member.UpdatedById = member.Client.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private string BuildImageUrl(string? imageName)
    {
        if (string.IsNullOrEmpty(imageName))
            return $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext!.Request.Host}/Images/default.png";

        var request = _httpContextAccessor.HttpContext!.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        return $"{baseUrl}/Images/{imageName}";
    }

    private (string firstName, string lastName) SplitName(string fullName)
    {
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var firstName = parts.FirstOrDefault() ?? "";
        var lastName = parts.Length > 1
            ? string.Join(" ", parts.Skip(1))
            : "";

        return (firstName, lastName);
    }
}