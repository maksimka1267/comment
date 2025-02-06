using comment.Data;
using comment.Interface;
using comment.Repository;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Retrieve the IConfiguration from the builder
var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
configuration.AddEnvironmentVariables();
configuration.AddCommandLine(args);

builder.Services.AddTransient < ICommentRepository, CommentRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("AppDbConnectionString")),
    ServiceLifetime.Scoped);
// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
