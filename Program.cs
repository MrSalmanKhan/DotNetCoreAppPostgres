using System.Security.Claims;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add EF Core + Postgres
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Google";
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;

    // Request extra user info
    options.Scope.Add("profile");
    options.Scope.Add("email");

    // Map profile picture and email claim
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.ClaimActions.MapJsonKey("picture", "picture", "url");

    options.SaveTokens = true;
    options.Events.OnCreatingTicket = async ctx =>
    {
        var db = ctx.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        var name = ctx.Principal!.Identity?.Name;
        var picture = ctx.Principal.FindFirst("picture")?.Value;

        if (email != null)
        {
            // Check if user exists
            var user = await db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Create new user with default role
                var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (defaultRole == null)
                {
                    defaultRole = new AppRole { Name = "User" };
                    db.Roles.Add(defaultRole);
                    await db.SaveChangesAsync();
                }

                user = new AppUser
                {
                    Email = email,
                    Name = name ?? email,
                    PictureUrl = picture
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();

                db.UserRoles.Add(new AppUserRole
                {
                    UserId = user.Id,
                    RoleId = defaultRole.Id
                });
                await db.SaveChangesAsync();
            }

            // Add role claims from DB
            var identity = (ClaimsIdentity)ctx.Principal.Identity!;
            foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    };

});

// --- Azure OpenAI Config ---
var endpoint = new Uri(builder.Configuration["AzureOpenAI:EndPoint"]!);
var deploymentName = builder.Configuration["AzureOpenAI:DeploymentName"]!;
var apiKey = builder.Configuration["AzureOpenAI:ApiKey"]!;

// Initialize the AzureOpenAIClient
builder.Services.AddSingleton(new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey)));

// Register ChatClient in DI
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<AzureOpenAIClient>();
    return client.GetChatClient(deploymentName);
});

builder.Services.AddHttpClient<WeatherService>();

//builder.Services.AddControllersWithViews()
//    .AddMicrosoftIdentityUI();
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();

builder.Services.AddScoped<IClaimsTransformation, DbRoleClaimsTransformer>();

var app = builder.Build();

// Automatically apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();  // applies pending migrations
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");

app.Run();