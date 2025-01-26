using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MindCare.Data;
using MindCare.Hubs;
using MindCare.Models;
using MindCare.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
    
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() 
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddScoped<ChatbotService>();
//builder.Services.Configure<OpenAISettings>(configuration.GetSection("OpenAI"));

builder.Services.AddScoped<UserActivityService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
/*builder.Services.Configure<NotificationConfiguration>(builder.Configuration.GetSection("NotificationConfiguration"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<NotificationConfiguration>>().Value);
builder.Services.AddHostedService<NotificationCleanupService>();*/

builder.Services.AddHttpClient("mpesa", c => {
    c.BaseAddress = new Uri("https://sandbox.safaricom.co.ke");
}
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); // Add authentication middleware
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<NotificationHub>("/notificationHub");




// Redirect unauthenticated users to the login page
app.MapGet("/", async context =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }

    // Redirect based on user role
    if (context.User.IsInRole("Student"))
    {
        context.Response.Redirect("/Student/Dashboard");
        return;
    }
    if (context.User.IsInRole("Therapist"))
    {
        context.Response.Redirect("/Therapist/Index");
        return;
    }
    if (context.User.IsInRole("Psychiatrist"))
    {
        context.Response.Redirect("/Psychiatrist/Dashboard");
    }
    if (context.User.IsInRole("Admin"))
    {
        context.Response.Redirect("/Admin/Dashboard");
        return;
    }

    //context.Response.Redirect("/Home/Index");
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



// Seed roles and admin user when the application starts
await SeedRolesAndAdminUser(app.Services);

app.Run();


static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
{
    using (var scope = serviceProvider.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var roles = new[] { "Admin", "Student", "Therapist", "Psychiatrist" };

        // Seed roles
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = "admin@admin.com";
        var adminPassword = "Admin@123";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin", // Set the FirstName
                LastName = "User",   // Set the LastName
                Role = "Admin"       // Set the Role explicitly
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
