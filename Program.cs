using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InfoPoint.Data;
using InfoPoint.Models;
using InfoPoint.Data.Seeders;
using InfoPoint.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => {
            sqlOptions.CommandTimeout(120); // 2 minutes timeout for long queries
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));

// Add Ulive database context (for card tokens, etc.)
builder.Services.AddDbContext<UliveDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UliveConnection"),
        sqlOptions => {
            sqlOptions.CommandTimeout(120); // 2 minutes timeout for long queries
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["GoogleAuthentication:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["GoogleAuthentication:ClientSecret"] ?? "";
        options.CallbackPath = "/signin-google";
        
        // Request additional scopes
        options.Scope.Add("profile");
        options.Scope.Add("email");
        
        // Save tokens for further API calls if needed
        options.SaveTokens = true;
    });

builder.Services.AddControllersWithViews();

// Add PDR service
builder.Services.AddScoped<IPDRService, PDRService>();

// Add OpenAI service with HttpClient
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Configure Areas routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed the database (only HR user for now - other tables need to be created first)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // var pdrService = scope.ServiceProvider.GetRequiredService<IPDRService>();
    // Don't use EnsureCreated with migrations - use migrations instead
    // context.Database.EnsureCreated();
    // await StaffSeeder.SeedAsync(context); // Skip - Staff table already exists in SQL Server
    // await pdrService.SeedQuestionsAsync(); // Skip - PDRQuestions table needs to be created first
    // await PDRSeeder.SeedAsync(context); // Skip for now - will fail without staff
    await HRSeeder.SeedAsync(context);
}

app.Run();
