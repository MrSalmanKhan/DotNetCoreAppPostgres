using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    // Tracks online users: ConnectionId -> UserName
    public readonly static Dictionary<string, string> OnlineUsers = new();

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendMessage(string message)
    {
        if (Context.User?.Identity?.Name == null)
            throw new Exception("User not authenticated");

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Name == Context.User.Identity.Name);
        user ??= new AppUser { Name = "Anonymous" };

        // Get profile picture from DB or claims
        var picture = user.PictureUrl ?? Context.User?.Claims?.FirstOrDefault(c => c.Type == "picture")?.Value
                      ?? "/images/default-avatar.png";

        var chatMessage = new ChatMessage
        {
            AppUserId = user.Id,
            PictureUrl = picture,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        await Clients.All.SendAsync(
            "ReceiveMessage",
            user.Name,
            chatMessage.PictureUrl,
            chatMessage.Message,
            chatMessage.Timestamp
        );
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity?.Name == null)
            throw new Exception("User not authenticated");

        var user = await _context.Users.SingleOrDefaultAsync(x => x.Name == Context.User.Identity.Name);
        if (user != null)
        {
            OnlineUsers[Context.ConnectionId] = user.Name;
            await BroadcastUserList();
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        OnlineUsers.Remove(Context.ConnectionId);
        await BroadcastUserList();
        await base.OnDisconnectedAsync(exception);
    }

    // Sends full user list with online status
    private async Task BroadcastUserList()
    {
        var allUsers = await _context.Users
            .Select(u => new
            {
                name = u.Name,
                isOnline = OnlineUsers.Values.Contains(u.Name),
                pictureUrl = u.PictureUrl ?? "/images/default-avatar.png"
            })
            .ToListAsync();

        await Clients.All.SendAsync("UpdateUserList", allUsers);
    }
}
