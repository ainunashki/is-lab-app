var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

List<Note> notes = new();
int idCounter = 1;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/health", () =>
{
    return Results.Json(new
    {
        status = "ok",
        time = DateTime.UtcNow
    });
});

app.MapGet("/version", (IConfiguration config) =>
{
    return Results.Json(new
    {
        name = config["App:Name"],
        version = config["App:Version"]
    });
});


app.MapPost("/api/notes", (Note note) =>
{
    if (string.IsNullOrWhiteSpace(note.Title))
        return Results.BadRequest("Title is required");

    note.Id = idCounter++;
    note.CreatedAt = DateTime.UtcNow;
    notes.Add(note);

    return Results.Ok(note);
});


app.MapGet("/api/notes", () => notes);


app.MapGet("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(x => x.Id == id);
    return note is null ? Results.NotFound() : Results.Ok(note);
});


app.MapDelete("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(x => x.Id == id);
    if (note == null)
        return Results.NotFound();

    notes.Remove(note);
    return Results.Ok();
});

app.MapGet("/db/ping", (IConfiguration config) =>
{
    var conn = config.GetConnectionString("Mssql");

    if (string.IsNullOrEmpty(conn))
        return Results.Problem("Connection string not set");

    return Results.Ok(new
    {
        status = "ok",
        connection = "configured"
    });
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class Note
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
}