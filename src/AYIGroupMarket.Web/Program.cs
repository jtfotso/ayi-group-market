using AYIGroupMarket.Web.Components;
using AYIGroupMarket.Infrastructure;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using AYIGroupMarket.Application;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WholesaleCustomer", policy => policy.RequireRole("Wholesale"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddLocalization();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AYIGroupMarket.Web.Services.CartSessionProvider>();
builder.Services.AddScoped<AYIGroupMarket.Web.Services.CartNotifier>();
builder.Services.AddCascadingAuthenticationState();

var supportedCultures = new[] { "fr", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

localizationOptions.RequestCultureProviders = new List<IRequestCultureProvider>
{
    new CookieRequestCultureProvider()
};

builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.DefaultRequestCulture = localizationOptions.DefaultRequestCulture;
    opt.SupportedCultures = localizationOptions.SupportedCultures;
    opt.SupportedUICultures = localizationOptions.SupportedUICultures;
    opt.RequestCultureProviders = localizationOptions.RequestCultureProviders;
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".dataprotection-keys")))
    .SetApplicationName("AYIGroupMarket");

var app = builder.Build();


app.UseRequestLocalization(localizationOptions);

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AYIGroupMarket.Infrastructure.Persistence.AppDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();

    await db.Database.MigrateAsync();
    await AYIGroupMarket.Infrastructure.Identity.RoleSeeder.SeedAsync(scope.ServiceProvider);
    await AYIGroupMarket.Infrastructure.Persistence.CatalogSeeder.SeedAsync(db);
    await AYIGroupMarket.Infrastructure.Persistence.ShippingSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/culture/set", (HttpContext httpContext, string culture, string redirectUri) =>
{
    httpContext.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

    return Results.LocalRedirect(redirectUri);
}).DisableAntiforgery();
app.MapPost("/account/register-submit", async (
    HttpContext httpContext,
    UserManager<AYIGroupMarket.Infrastructure.Identity.ApplicationUser> userManager,
    SignInManager<AYIGroupMarket.Infrastructure.Identity.ApplicationUser> signInManager) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var firstName = form["firstName"].ToString();
    var lastName = form["lastName"].ToString();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var user = new AYIGroupMarket.Infrastructure.Identity.ApplicationUser
    {
        UserName = email,
        Email = email,
        FirstName = firstName,
        LastName = lastName
    };

    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, "Customer");
        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }

    var errors = string.Join(" ", result.Errors.Select(e => e.Description));
    return Results.LocalRedirect($"/account/register?error={Uri.EscapeDataString(errors)}" +
        (string.IsNullOrEmpty(returnUrl) ? "" : $"&returnUrl={Uri.EscapeDataString(returnUrl)}"));
}).DisableAntiforgery();
app.MapPost("/account/login-submit", async (
    HttpContext httpContext,
    SignInManager<AYIGroupMarket.Infrastructure.Identity.ApplicationUser> signInManager) =>
{
    var form = await httpContext.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
    //Console.WriteLine($"[Login] Succeeded={result.Succeeded}, IsNotAllowed={result.IsNotAllowed}, IsLockedOut={result.IsLockedOut}");
    if (result.Succeeded)
        return Results.LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);

    var error = Uri.EscapeDataString("Incorrect email or password.");
    return Results.LocalRedirect($"/account/login?error={error}" +
        (string.IsNullOrEmpty(returnUrl) ? "" : $"&returnUrl={Uri.EscapeDataString(returnUrl)}"));
}).DisableAntiforgery();
app.MapPost("/account/logout", async (
    SignInManager<AYIGroupMarket.Infrastructure.Identity.ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/");
}).DisableAntiforgery();
app.Run();
