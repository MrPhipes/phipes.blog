using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Phipes.Blog;
using Phipes.Blog.Data;
using Phipes.Blog.DependencyInjection;
using WebApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// --- Identidad de DEMO ---------------------------------------------------------
// El sample usa una cookie simple solo para demostrar el panel de administración.
// Un host real (phipes.web) enchufa ASP.NET Core Identity o PacificDev.Identity aquí.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => o.LoginPath = "/dev-login");

// --- Motor de blog -------------------------------------------------------------
builder.Services
    .AddPhipesBlog(o =>
    {
        o.DefaultTenantId = "demo";
        o.AllowExternalAuthors = true;
        o.ModerateComments = true;
    })
    .UseDatabase(db => db.UseSqlite("Data Source=phipesblog.db"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();      // sirve los assets del paquete bajo /_content/Phipes.Blog
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Login de desarrollo: firma una sesión con el rol admin del blog para probar el panel.
app.MapGet("/dev-login", async (HttpContext ctx) =>
{
    var identity = new ClaimsIdentity(
    [
        new Claim(ClaimTypes.NameIdentifier, "dev-admin"),
        new Claim("name", "Administrador Demo"),
        new Claim(ClaimTypes.Role, "BlogAdmin"),
    ], CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Redirect("/admin/blog");
});

// Crea la base y carga contenido de ejemplo en el primer arranque.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PhipesBlogDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SampleData.SeedAsync(db);
}

app.Run();
