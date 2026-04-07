using Microsoft.EntityFrameworkCore;
using cuahanggiay.Data;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- 1. Cấu hình Dịch vụ (Services) ---
        builder.Services.AddControllersWithViews();

        // [THÊM MỚI] Cấu hình Xác thực (Authentication) bằng Cookie
        builder.Services.AddAuthentication("CustomerSecurityScheme")
            .AddCookie("CustomerSecurityScheme", options =>
            {
                options.Cookie.Name = ".VuaGiayHieu.Auth";
                options.LoginPath = "/Account/Login"; // Chuyển hướng về đây nếu chưa đăng nhập
                options.AccessDeniedPath = "/Account/AccessDenied"; // Nếu không có quyền truy cập
                options.ExpireTimeSpan = TimeSpan.FromDays(7); // Giữ đăng nhập trong 7 ngày
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

        // Cấu hình Session cho Giỏ hàng
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = ".VuaGiayHieu.Session";
        });

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Chưa cấu hình 'DefaultConnection'.");

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        var app = builder.Build();

        // --- 2. Khởi tạo Database ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
                cuahanggiay.Data.SeedData.Initialize(context);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "LỖI HỆ THỐNG: Không thể khởi tạo cơ sở dữ liệu.");
            }
        }

        // --- 3. Cấu hình Pipeline (Middleware) ---
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();

        // [QUAN TRỌNG] UseAuthentication phải nằm TRƯỚC UseAuthorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}