using Microsoft.AspNetCore.SignalR;
using SharedCanvasWeb.DTOs;
using SharedCanvasWeb.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

var strokes = new List<StrokeDto>();
var strokesLock = new object();

app.MapHub<DrawingHub>("/hubs/drawing");

app.MapPost("/api/rooms/{room}/strokes", (string room, StrokeDto stroke) =>
{
    var s = stroke with { Room = room, CreatedAtTicks = DateTime.UtcNow.Ticks };
    lock (strokesLock)
    {
        strokes.Add(s);
    }
    return Results.Ok();
});

app.MapGet("/api/rooms/{room}/updates", (string room, long sinceTicks) =>
{
    List<StrokeDto> result;
    lock (strokesLock)
    {
        result = strokes
            .Where(s => s.Room == room && s.CreatedAtTicks > sinceTicks)
            .OrderBy(s => s.CreatedAtTicks)
            .ToList();
    }
    return Results.Ok(result);
});

app.MapPost("/api/rooms/{room}/clear",
    async (string room, IHubContext<DrawingHub> hub) =>
    {
        lock (strokesLock)
        {
            strokes.RemoveAll(s => s.Room == room);
        }
        await hub.Clients.Group(room).SendAsync("CanvasCleared");

        return Results.Ok();
    });

app.MapGet("/api/rooms/{room}/updates-long", async (string room, long sinceTicks) =>
{
    for (var i = 0; i < 10; i++)
    {
        List<StrokeDto> result;
        lock (strokesLock)
        {
            result = strokes
                .Where(s => s.Room == room && s.CreatedAtTicks > sinceTicks)
                .OrderBy(s => s.CreatedAtTicks)
                .ToList();
        }

        if (result.Count > 0)
        {
            return Results.Ok(result);
        }

        await Task.Delay(1000);
    }
    return Results.Ok(new List<StrokeDto>());
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();
