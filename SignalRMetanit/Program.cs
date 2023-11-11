using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using SignalRApp; // пространство имен класса ChatHub
using Microsoft.EntityFrameworkCore;
using SignalRMetanit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OnlineChatContext>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/register");
builder.Services.AddAuthorization();

builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<OnlineChatContext>();

    //var adminRole = context.Users.FirstOrDefault(u => u.Role == "admin");
    //var userRole = context.Users.FirstOrDefault(u => u.Role == "user");
    var people = context.Users.ToList();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async context =>
    await SendHtmlAsync(context, "wwwroot/login.html"));

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("name") || !form.ContainsKey("password"))
        return Results.BadRequest("Имя и/или пароль не установлены");
    string name = form["name"];
    string password = form["password"];

    var dbContext = context.RequestServices.GetRequiredService<OnlineChatContext>();
    User? person = dbContext.Users.FirstOrDefault(u => u.Name == name && u.Password == password);
    if (person is null) return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, person.Name),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
    };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);

    // Добавьте этот отладочный код:
    var userRole = context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value;
    Console.WriteLine($"User Role: {userRole}");

    await context.SignInAsync(claimsPrincipal);
    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/register", async context =>
    await SendHtmlAsync(context, "wwwroot/register.html"));

app.MapPost("/register", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    if (!form.ContainsKey("name") || !form.ContainsKey("password"))
        return Results.BadRequest("Имя и/или пароль не установлены");
    string name = form["name"];
    string password = form["password"];
    string role = "admin";

    // Создайте нового пользователя с ролью "user"
    var newUser = new User
    {
        Name = name,
        Password = password,
        Role = role
    };

    var dbContext = context.RequestServices.GetRequiredService<OnlineChatContext>();
    dbContext.Users.Add(newUser);
    await dbContext.SaveChangesAsync();

    return Results.Redirect("/login");
});


app.MapGet("/", [Authorize] async (HttpContext context) =>
{
    var userRole = context.User.FindFirst(ClaimsIdentity.DefaultRoleClaimType)?.Value;

    if (userRole == "admin")
    {
        await SendHtmlAsync(context, "wwwroot/admin.html");
        return;
    }
    else
    {
        await SendHtmlAsync(context, "wwwroot/index.html");
        return;
    }
});

app.MapGet("/admin", [Authorize(Roles = "admin")] async (HttpContext context) =>
    await SendHtmlAsync(context, "wwwroot/admin.html"));


app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapHub<ChatHub>("/chat");
app.Run();

async Task SendHtmlAsync(HttpContext context, string path)
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(path);
}
