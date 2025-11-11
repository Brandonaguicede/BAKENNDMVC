using AppPrint_and_Wear.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =============================================
// ?? Configuración de conexión a SQL Server local
// =============================================
builder.Services.AddDbContext<ApplicationDBContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =============================================
// ?? Configurar controladores y vistas
// =============================================
builder.Services.AddControllersWithViews();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.MaxDepth = 64;
        options.JsonSerializerOptions.DefaultBufferSize = 2048000; // 2 MB
    });

// =============================================
// ?? Habilitar el uso de sesiones
// =============================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ? Agregar soporte para acceso a HttpContext desde controladores
builder.Services.AddHttpContextAccessor(); // <-- AGREGA ESTA LÍNEA

var app = builder.Build();

// =============================================
// ?? Configuración del pipeline HTTP
// =============================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ? Middleware de sesiones — debe ir ANTES de Authorization
app.UseSession();

app.UseAuthorization();

// =============================================
// ?? Rutas MVC
// =============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// =============================================
// ?? Ejecutar la aplicación
// =============================================
app.Run();
