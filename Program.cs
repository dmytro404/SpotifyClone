using Microsoft.EntityFrameworkCore;
using SpotifyClone.Data;
using SpotifyClone.Middleware.Auth;
using SpotifyClone.Services.Auth;
using SpotifyClone.Services.Kdf;
using SpotifyClone.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

builder.Services.AddSingleton<IKdfService, PbKdf1Service>();
builder.Services.AddSingleton<IStorageService, DiskStorageService>();
builder.Services.AddScoped<DataAccessor>();
builder.Services.AddScoped<IAuthService, SessionAuthService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options => { options.AddDefaultPolicy(policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); ; }); });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.UseAuthorization();

app.UseSession();
app.UseSessionAuth();
app.UseJwtAuth();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
