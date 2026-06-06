using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Hubs;

[Authorize]
public class GateHub(ApplicationDbContext context) : Hub<IGateHubClient>
{
    private readonly ApplicationDbContext _context = context;

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return;

        var gateIds = await _context.Gates
            .Where(g => g.Client.UserId == userId && !g.IsDeleted)
            .Select(g => g.Id)
            .ToListAsync();

        foreach (var gateId in gateIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"gate-{gateId}");
        }

        await base.OnConnectedAsync();
    }

    public async Task SubscribeToGate(int gateId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"gate-{gateId}");
    }

    public async Task UnsubscribeFromGate(int gateId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"gate-{gateId}");
    }
}
