using System.Data;
using Microsoft.Data.Sqlite;
using Dapper;
using WanderEase.Models;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbConnection>(b => new SqliteConnection("Data Source=./wwwroot/destinations.db"));

var app = builder.Build();
app.UseStaticFiles();

HashSet<int> selectedDestinations = new HashSet<int>();
Random random = new Random();

app.MapGet("/destinations/all-seasons", async (IDbConnection db) =>
{
    string[] seasons = { "Winter", "Summer", "Spring", "Autumn" };
    string html = "";
    Console.WriteLine(seasons);

    foreach (var season in seasons)
    {
        var destinations = await db.QueryAsync("SELECT * FROM destinations WHERE Season = @Season ORDER BY RANDOM() LIMIT 1", new { Season = season });
        var randomDestination = destinations.FirstOrDefault();

        html += $@"
        <div class=""card card-season"">
            <img src=""../{randomDestination?.Image}"" class=""card-img-top"" alt=""..."">
            <div class=""card-body"">
                <p class=""card-season-word"">{randomDestination?.Season}</p>
            </div>
            <div class=""location-text"">
                <p class=""location"">{randomDestination?.Name}</p>
            </div>
        </div>";
    }

    return Results.Content(html, "text/html");
});

app.MapGet("/destinations/names", async (IDbConnection db) =>
{
    var destinations = await db.QueryAsync("SELECT Id, Name FROM destinations;");
    string html = "";
    foreach (var dest in destinations)
    {
        html += $@"
            <option value=""{dest.Id}"">{dest.Name}</option>";
        Console.WriteLine(dest.Id);
    }
    return Results.Content(html, "text/html");
});

app.MapPost("/destinations", async (HttpContext context, IDbConnection db) =>
{
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    var destinationName = form["destination"];
    var season = form["season"];

    Console.WriteLine(season);


    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }
    if (destinationName == "")
    {
        return Results.BadRequest("Destination not Received");
    }

    var fileId = Guid.NewGuid().ToString();
    Directory.CreateDirectory("wwwroot/uploads");
    var filePath = "wwwroot/uploads/" + fileId + Path.GetExtension(file.FileName);
    string relativePath = filePath.Replace("wwwroot/", "");
    using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }

    var sql = "INSERT INTO destinations (Id, Name, Image, Season) VALUES (@Id, @Name, @Image, @Season)";
    var rows = await db.ExecuteAsync(sql, new { Id = fileId, Name = destinationName, Image = relativePath, Season = season });
    Console.WriteLine("The number of affected rows: " + rows);
    return Results.NoContent();
});

app.MapDelete("/destinations/{id}", async (string id, IDbConnection db) =>
{
    Console.WriteLine(id);
    var sql = "DELETE FROM destinations WHERE Id = @Id;";
    var rowsAffected = await db.ExecuteAsync(sql, new { Id = id });

    return rowsAffected > 0 ? Results.Ok("Successfuly deleted the destination") : Results.NotFound();
});

app.Run();
