using SharedCanvasWeb.DTOs;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

namespace SharedCanvasWeb.Hubs;

public class DrawingHub : Hub
{
    public async Task JoinRoom(string room)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
    }
    public async Task SendStroke(string room, StrokeDto stroke)
    {
        var s = stroke with { Room = room, CreatedAtTicks = DateTime.UtcNow.Ticks };
        await Clients.OthersInGroup(room).SendAsync("StrokeReceived", s);
    }

    public async Task ClearCanvas(string room)
    {
        await Clients.Group(room).SendAsync("CanvasCleared");
    }

    public async Task Ping(long clientTicks)
    {
        await Clients.Caller.SendAsync("Pong", clientTicks, DateTime.UtcNow.Ticks);
    }
}
