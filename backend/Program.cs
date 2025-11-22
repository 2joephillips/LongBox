using System.Numerics;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

// get filepath from configuration
var comicsPath = builder.Configuration.GetValue<string>("ComicsPath");
var databasePath = builder.Configuration.GetValue<string>("DatabasePath");

Console.WriteLine($"Using comics path: {comicsPath}");
Console.WriteLine($"Using database path: {databasePath}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();

