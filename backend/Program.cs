var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

// get filepath from configuration
var filePath = builder.Configuration.GetValue<string>("FilePath");
Console.WriteLine($"Using file path: {filePath}");
if (Directory.Exists(filePath))
{
    Console.WriteLine("Directory exists.");
}
else
{
    Console.WriteLine("Directory does not exist.");
}
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();

