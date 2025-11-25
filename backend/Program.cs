using System.Numerics;
using System.IO;
using LiteDB;
using Serilog;
using Serilog.Events;
using Serilog.Templates;
using Serilog.Templates.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

// get filepath from configuration
var comicsPath = builder.Configuration.GetValue<string>("ComicsPath");
var databasePath = builder.Configuration.GetValue<string>("DatabasePath");

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

Log.Information("Comics path: {ComicsPath}", comicsPath);
Log.Information("Database path: {DatabasePath}", databasePath);

builder.Services.AddSingleton(sp =>
{

    // Ensure folder exists
    var filePath = databasePath ?? "data/db/litedb.db";
    if (!Directory.Exists(filePath))
        Directory.CreateDirectory(filePath);

    return new LiteDatabase(new ConnectionString
    {
        Filename = filePath,
        Connection = ConnectionType.Shared   // <– important
    });
});

builder.Services.Configure<WatcherOptions>(builder.Configuration.GetSection("Watcher"));
builder.Services.AddSingleton<FileEventChannel>();
builder.Services.AddSingleton<FileWatcher>();
builder.Services.AddMemoryCache();
builder.Services.AddHostedService<FileWatcherService>();
builder.Services.AddTransient<IFileProcessor, ComicFileProcessor>();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

app.Run();
