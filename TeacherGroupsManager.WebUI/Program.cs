using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Constants;
using TeacherGroupsManager.Data.Context;
using TeacherGroupsManager.Services;
using TeacherGroupsManager.WebUI.Infrastructure;
using TeacherGroupsManager.WebUI.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddLocalization();

builder.Services.AddDbContext<TeacherGroupsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<TeacherGroupsManager.Data.Repositories.ICurrentUserContext, CurrentUserContext>();
builder.Services.AddTeacherGroupsServices();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = AppConstants.AuthCookieName;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionPolicy.All)
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("Permission", permission));
    }
});

var app = builder.Build();

await app.InitializeDatabaseAsync();

var supportedCultures = new[]
{
    new CultureInfo(AppConstants.ArabicCulture),
    new CultureInfo("en-US"),
    new CultureInfo("fr-FR")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(AppConstants.ArabicCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
