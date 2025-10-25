using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SpotifyClone.Data;
using SpotifyClone.Middleware.Auth;
using SpotifyClone.Services.Auth;
using SpotifyClone.Services.Kdf;
using SpotifyClone.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IKdfService, PbKdf2Service>();
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddScoped<DataAccessor>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, SessionAuthService>();
builder.Services.AddSingleton<IStorageService, DiskStorageService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider("C:/storage/ASP32/"),
    RequestPath = "/storage"
});

app.UseRouting();
app.UseCors();

app.UseSession();
app.UseSessionAuth();
app.UseJwtAuth();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();