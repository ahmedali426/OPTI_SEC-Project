using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.Gates;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.GateServices;

public class GateService(ApplicationDbContext context) : IGateService
{
    private readonly ApplicationDbContext _context = context;

    private async Task<Client?> GetClient(string userId, CancellationToken cancellationToken)
    {
        return await _context.Clients
            .FirstOrDefaultAsync(x => x.UserId == userId && !x.IsDeleted, cancellationToken);
    }

    public async Task<Result<GateResponse>> CreateAsync(string userId, GateRequest request, CancellationToken cancellationToken)
    {
        var client = await GetClient(userId, cancellationToken);

        if (client is null)
            return Result.Failure<GateResponse>(ClientErrors.NotFound);

        var nameExists = await _context.Gates
            .AnyAsync(x =>
                x.Name == request.Name &&
                !x.IsDeleted,
                cancellationToken);

        if (nameExists)
            return Result.Failure<GateResponse>(GateErrors.DuplicateName);

        var gate = new Gate
        {
            Name = request.Name,
            Location = request.Location,
            ClientId = client.Id,
            DeviceId = request.DeviceId,
            DeviceApiKey = GenerateDeviceApiKey(),
            Status = GateStatus.Online,
            MaxFailedAttempts = 3,
            PasswordHash = request.Password,
            SilentAlarmHash = request.SilentAlarm
        };

        await _context.Gates.AddAsync(gate, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);



        var response = new GateResponse(
            gate.Id,
            gate.Name,
            gate.Location,
            gate.DeviceId,
            gate.Status.ToString(),
            gate.MaxFailedAttempts,
            gate.PasswordHash,
            gate.SilentAlarmHash,
            gate.DeviceApiKey
        );

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<GateResponseAll>>> GetAllAsync(string userId, CancellationToken cancellationToken)
    {
        var client = await GetClient(userId, cancellationToken);

        if (client is null)
            return Result.Failure<IEnumerable<GateResponseAll>>(ClientErrors.NotFound);

        var gates = await _context.Gates
            .AsNoTracking()
            .Where(x => x.ClientId == client.Id && !x.IsDeleted)
            .Select(x => new GateResponseAll(
                x.Id,
                x.Name,
                x.Location
            ))
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<GateResponseAll>>(gates);
    }

    public async Task<Result<GateResponse>> GetByIdAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var client = await GetClient(userId, cancellationToken);

        if (client is null)
            return Result.Failure<GateResponse>(ClientErrors.NotFound);

        var gate = await _context.Gates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.ClientId == client.Id, cancellationToken);

        if (gate is null)
            return Result.Failure<GateResponse>(GateErrors.NotFound);

        var response = new GateResponse(
            gate.Id,
            gate.Name,
            gate.Location,
            gate.DeviceId,
            gate.Status.ToString(),
            gate.MaxFailedAttempts,
            gate.PasswordHash,
            gate.SilentAlarmHash,
            gate.DeviceApiKey
        );

        return Result.Success(response);
    }

    public async Task<Result> UpdateAsync(string userId, int id, GateRequest request, CancellationToken cancellationToken)
    {
        var client = await GetClient(userId, cancellationToken);

        if (client is null)
            return Result.Failure(ClientErrors.NotFound);

        var gate = await _context.Gates
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted && x.ClientId == client.Id, cancellationToken);

        if (gate is null)
            return Result.Failure(GateErrors.NotFound);

        var nameExists = await _context.Gates
            .AnyAsync(x =>
                x.Id != id &&
                x.ClientId == client.Id &&
                x.Name == request.Name &&
                !x.IsDeleted,
                cancellationToken);

        if (nameExists)
            return Result.Failure(GateErrors.DuplicateName);

        gate.Name = request.Name;

        gate.Location = request.Location;

        gate.DeviceId = request.DeviceId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var client = await GetClient(userId, cancellationToken);

        if (client is null)
            return Result.Failure(ClientErrors.NotFound);

        var gate = await _context.Gates
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.ClientId == client.Id &&
                !x.IsDeleted,
                cancellationToken);

        if (gate is null)
            return Result.Failure(GateErrors.NotFound);

        gate.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<int>> CountGatesAsync(string userId, CancellationToken cancellationToken)
    {
        var client = await GetClient(userId, cancellationToken);

        if (client is null)
            return Result.Failure<int>(ClientErrors.NotFound);

        var count = await _context.Gates
            .Where(x => x.ClientId == client.Id && !x.IsDeleted)
            .CountAsync(cancellationToken);

        return Result.Success(count);

    }
    private string GenerateDeviceApiKey()
    {
        // توليد مصفوفة بايتات عشوائية (32 بايت = 256 بت)
        var keyBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(keyBytes);
        }

        // تحويلها إلى نص (Base64 أو Hex)
        // يفضل إزالة الرموز الخاصة لتسهيل التعامل معه في الروابط والـ Headers
        string apiKey = Convert.ToBase64String(keyBytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        return apiKey;
    }
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
   
}
