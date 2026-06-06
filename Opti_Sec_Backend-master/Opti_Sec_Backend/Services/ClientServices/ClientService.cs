using System.Buffers.Text;
using System.Linq;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Abstractions.Consts;
using Opti_Sec_Backend.Contracts.Clients;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Helper;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.FileServices;

namespace Opti_Sec_Backend.Services.ClientServices;

public class ClientService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IFileService fileService,
    EmailHelper emailHelper
    ) : IClientService
{
    private readonly ApplicationDbContext _context = context;

    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private readonly IFileService _fileService = fileService;

    private readonly EmailHelper _emailHelper = emailHelper;

    public async Task<Result<ClientResponse>> CreateAsync(ClientRequest request, CancellationToken cancellationToken)
    {
        var userIsExist = await _userManager.FindByEmailAsync(request.Email);

        if (userIsExist is not null)
            return Result.Failure<ClientResponse>(ClientErrors.DuplicateClientEmail);

        var usernameExist = await _context.Clients
            .Include(x => x.User)
            .AnyAsync(x => x.User.UserName == request.UserName, cancellationToken);

        if(usernameExist)
            return Result.Failure<ClientResponse>(ClientErrors.DuplicateUserName);

        string? imageName = null;
        if (request.Image is not null)
        {
            imageName = await _fileService.UploadImageAsync(request.Image, cancellationToken);
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.UserName,
            FName = request.FName,
            LName = request.LName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault();
            return Result.Failure<ClientResponse>(new Error(error!.Code,error.Description,StatusCodes.Status400BadRequest));

        }

        var roleResult = await _userManager.AddToRoleAsync(user, DefaultRoles.Member);

        if (!roleResult.Succeeded)
        {
            var error = roleResult.Errors.FirstOrDefault();
            return Result.Failure<ClientResponse>(
                new Error(error!.Code, error.Description, StatusCodes.Status400BadRequest));
        }


        var client = new Client
        {
            FName = request.FName,
            LName = request.LName,
            PhoneNumber = request.PhoneNumber,
            ImageUrl = imageName,
            UserId = user.Id
        };

        await _context.Clients.AddAsync(client, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailHelper.CreateUserEmail(
                request.Email,
                $"{request.FName} {request.LName}",
                request.Password
            );
        }
        catch
        {
            // Ignore email failure so client creation succeeds
        }

        var response = new ClientResponse
            (
            client.Id,
            $"{client.FName} {client.LName}",
            client.User.Email!,
            client.User.UserName!,
            client.User.PhoneNumber!,
            BuildImageUrl(client.ImageUrl)
            );

        return Result.Success(response);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(x => x.User)
            .Include(x => x.Members)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (client is null)
            return Result.Failure(ClientErrors.NotFound);

        // Delete members first (Client→Member is Restrict due to SQL Server limitation)
        // Member→AccessLog is Cascade, so AccessLogs are auto-deleted by the DB
        _context.Members.RemoveRange(client.Members);

        // Delete the client (Client→Gate is Cascade, Gate→AccessLog is Cascade)
        // So Gates and their AccessLogs are auto-deleted by the DB
        _context.Clients.Remove(client);

        // Delete the associated ApplicationUser
        await _userManager.DeleteAsync(client.User);

        // delete image
        if (!string.IsNullOrEmpty(client.ImageUrl))
        {
            await _fileService.DeleteImageAsync(client.ImageUrl);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<GetAllClientResponse>>> GetAllAsync(string? search,CancellationToken cancellationToken)
    {
        var query = _context.Clients
        .Where(x => !x.IsDeleted)
        .AsQueryable();

            
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();

            query = query.Where(x =>
                EF.Functions.Like(x.FName.ToLower(), $"%{search}%") ||
                EF.Functions.Like(x.LName.ToLower(), $"%{search}%")
            );
        }

        var clients = await query
            .Select(x => new
            {
                x.Id,
                Name = x.FName + " " + x.LName,
                x.ImageUrl
            })
            .ToListAsync(cancellationToken);

        var response = clients.Select(x => new GetAllClientResponse(
            x.Id,
            x.Name,
            BuildImageUrl(x.ImageUrl)
        ));

        return Result.Success<IEnumerable<GetAllClientResponse>>(response);
    }

    public async Task<Result<ClientResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure<ClientResponse>(ClientErrors.NotFound);


        var response = new ClientResponse
            (
            client.Id,
            $"{client.FName} {client.LName}",
            client.User.Email!,
            client.User.UserName!,
            client.User.PhoneNumber!,
            BuildImageUrl(client.ImageUrl)
            );

        return Result.Success(response);
    }

    public async Task<Result> UpdateAsync(int id, UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (client is null)
            return Result.Failure(ClientErrors.NotFound);

        var emailExist = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.Email == request.Email && x.Id != id, cancellationToken);

        if (emailExist is not null)
            return Result.Failure(ClientErrors.DuplicateClientEmail);

        var usernameExist = await _context.Clients
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.User.UserName == request.UserName && x.Id != id, cancellationToken);

        if(usernameExist is not null)
            return Result.Failure(ClientErrors.DuplicateUserName);


        if (request.Image is not null)
        {
            client.ImageUrl = await _fileService.UpdateImageAsync(client.ImageUrl, request.Image, cancellationToken);
        }

        var (firstName, lastName) = SplitName(request.Name);

        client.FName = firstName;
        client.LName = lastName;

        client.PhoneNumber = request.PhoneNumber;

        client.User.Email = request.Email;
        client.User.UserName = request.UserName;
        client.User.PhoneNumber = request.PhoneNumber;

        await _userManager.UpdateAsync(client.User);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<int>> CountAsync(CancellationToken cancellationToken)
    {
        var clientsCount = await _context.Clients.Where(x => !x.IsDeleted).CountAsync(cancellationToken);

        return Result.Success(clientsCount);
    }

    public async Task<Result<IEnumerable<GetAllClientResponseForAI>>> GetAllForAIAsync(CancellationToken cancellationToken)
    {
        var clients = await _context.Clients
         .Select(x => new
         {
             x.Id,
             Name = x.FName + " " + x.LName,
             x.User.UserName,
             x.ImageUrl,
             x.IsDeleted
         })
         .ToListAsync(cancellationToken);

            var response = clients.Select(x => new GetAllClientResponseForAI(
                x.Id,
                x.Name,
                x.UserName!,
                BuildImageUrl(x.ImageUrl),
                x.IsDeleted
            ));

        return Result.Success<IEnumerable<GetAllClientResponseForAI>>(response);
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
