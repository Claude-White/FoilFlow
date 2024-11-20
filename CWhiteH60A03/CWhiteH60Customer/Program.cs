using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using CWhiteH60Customer.DAL;
using CWhiteH60Customer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/* Database Context Dependency Injection */
// var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
// var dbName = Environment.GetEnvironmentVariable("DB_NAME");
// var dbPassword = Environment.GetEnvironmentVariable("DB_SA_PASSWORD");
// var connectionString = $"Data Source={dbHost};Initial Catalog={dbName};User ID=sa;Password={dbPassword};TrustServerCertificate=True;";
// builder.Services.AddDbContext<H60AssignmentDbCWContext>(opt => opt.UseSqlServer(connectionString));
/* ===================================== */

var connectionString = builder.Configuration.GetConnectionString("MyConnection") ?? throw new InvalidOperationException("Connection string 'H60AssignmentDbCWContextConnection' not found.");
builder.Services.AddDbContext<H60AssignmentDbCWContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<H60AssignmentDbCWContext>();

builder.Services.ConfigureApplicationCookie(options => {
    options.Cookie.Name = "CustomerAuth";
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});


builder.Services.AddAuthorization();

builder.Services.AddScoped<IProductRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IStoreRepository<ProductCategory>, ProdCatRepository>();
builder.Services.AddScoped<ICustomerRepository<Customer>, CustomerRepository>();
builder.Services.AddScoped<IPartialViewUtility, PartialViewUtility>();

builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews();

builder.Services.AddNotyf(config=>
    {
        config.DurationInSeconds = 5;
        config.IsDismissable = true;
        config.Position = NotyfPosition.BottomRight; 
    }
);

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseNotyf();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();