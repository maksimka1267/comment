using comment.Data.Model;
using comment.Data;
using comment.Interface;
using comment.Repository;
using comment.Services.Interface;
using comment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

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

// Регистрация QueueService для CommentQueueItem
builder.Services.AddSingleton<IQueueService<CommentQueueItem>, QueueService<CommentQueueItem>>();

// Регистрация фонового процесса для обработки очереди
builder.Services.AddHostedService<CommentQueueProcessor>();

// Добавляем MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Добавляем поддержку базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AppDbConnectionString")));

// Добавляем поддержку Razor Pages и контроллеров с представлениями
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Регистрируем сервис для WebSocket
builder.Services.AddSingleton<WebSocketHandler>();

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

// Обработчик WebSocket
app.UseWebSockets();

// Подключаем обработчик WebSocket для пути /ws
app.Map("/ws", (HttpContext context, WebSocketHandler webSocketHandler) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        webSocketHandler.HandleConnections(context);  // Передаем контекст в обработчик
    }
    else
    {
        context.Response.StatusCode = 400;  // Возвращаем ошибку, если запрос не WebSocket
    }
});

// Настроим маршруты для контроллеров и Razor Pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();
