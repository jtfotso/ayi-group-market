using AYIGroupMarket.Application;
using AYIGroupMarket.Infrastructure;
using AYIGroupMarket.Admin.Components;
using Microsoft.AspNetCore.Identity;
using AYIGroupMarket.Application.Abstractions;
using AYIGroupMarket.Infrastructure.Storage;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WholesaleCustomer", policy => policy.RequireRole("Wholesale"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

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
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "shared-uploads");
Directory.CreateDirectory(uploadsPath); // ensure it exists even on first run

/* app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
}); */

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
    return Results.LocalRedirect($"/account/register?error={Uri.EscapeDataString(errors)}");
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

    if (result.Succeeded)
        return Results.LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);

    var error = Uri.EscapeDataString("Incorrect email or password.");
    return Results.LocalRedirect($"/account/login?error={error}");
}).DisableAntiforgery();

app.MapPost("/account/logout", async (
    SignInManager<AYIGroupMarket.Infrastructure.Identity.ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("/");
}).DisableAntiforgery();

app.MapGet("/orders/export.csv", async (
    ISender sender, DateTime? start, DateTime? end, CancellationToken cancellationToken) =>
{
    var startDate = start ?? DateTime.Today.AddMonths(-1);
    var endDate = (end ?? DateTime.Today).AddDays(1).AddTicks(-1);

    var csvBytes = await sender.Send(new AYIGroupMarket.Application.Features.Admin.ExportOrdersCsv.ExportOrdersCsvQuery(startDate, endDate), cancellationToken);

    return Results.File(csvBytes, "text/csv", $"orders-export-{DateTime.Today:yyyy-MM-dd}.csv");
}).RequireAuthorization("RequireAdmin");

app.Run();
