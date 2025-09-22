using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages()
    {
        var messages = await _context.ChatMessages
            .Include(x => x.AppUser) // include user info
            .OrderBy(m => m.Timestamp)
            .Take(100)
            .Select(m => new
            {
                name = m.AppUser.Name,
                picture = m.AppUser.PictureUrl,
                message = m.Message,
                timestamp = m.Timestamp
            })
            .ToListAsync();

        return Json(messages);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Get list of online usernames from ChatHub (you might store it in a service)
        var onlineUserNames = ChatHub.OnlineUsers.Values.Distinct().ToHashSet();

        var users = await _context.Users
            .Select(u => new
            {
                name = u.Name,
                picture = u.PictureUrl ?? "/images/default-avatar.png",
                isonline = onlineUserNames.Contains(u.Name) // check if user is online
            })
            .ToListAsync();

        return Json(users);
    }
}
