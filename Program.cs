using Microsoft.EntityFrameworkCore;
using OkulTakipSistemi.Data;
using OkulTakipSistemi.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ============================================================
// SESSION
// ============================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();

// ============================================================
// EMAIL SERVICE
// ============================================================

builder.Services.AddScoped<EmailService>();

// ============================================================
// APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// ERROR HANDLING
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ============================================================
// HTTPS
// ============================================================


// ============================================================
app.UseHttpsRedirection();

// STATIC FILES
// ============================================================

app.UseStaticFiles();

// ============================================================
// ROUTING
// ============================================================

app.UseRouting();

// ============================================================
// SESSION
// ============================================================

app.UseSession();

// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();

// ============================================================
// DEFAULT ROUTE
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}"
);

// ============================================================
// RUN
// ============================================================

app.Run();