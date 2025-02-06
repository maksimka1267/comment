using comment.Data;
using comment.Interface;
using comment.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Загружаем конфигурацию
var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
configuration.AddEnvironmentVariables();
configuration.AddCommandLine(args);

// Регистрация зависимостей
builder.Services.AddTransient<ICommentRepository, CommentRepository>();
builder.Services.AddTransient<IAttachmentRepository, AttachmentRepository>();

// Добавляем поддержку базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AppDbConnectionString")),
    ServiceLifetime.Scoped);

// Добавляем поддержку Razor Pages и контроллеров с представлениями
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Конфигурация middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Настраиваем маршрутизацию
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
