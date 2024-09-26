using Microsoft.EntityFrameworkCore;
using Dapper;
using MySqlConnector;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 23))));


//let me add also dapper for direct sql
builder.Services.AddScoped<MySqlConnection>(sp =>
new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
);
//that is it. no need any special interface 






var app = builder.Build();

app.MapGet("/", () => "Hello World!");


app.MapGet("/connectioncheck", async (AppDbContext db) =>
{

    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok("Connection Successfull");
    }
    catch (Exception ex)
    {
        return Results.Problem("failed", ex.Message);
    }

});
//get pets used EFC 
app.MapGet("/getallpets", async (AppDbContext db) =>
{
    try
    {
        var pets = await db.Pets.ToListAsync();
        return Results.Ok(pets);
    }
    catch (Exception ex)
    {
        return Results.Problem("failed", ex.Message);
    }
});

//get Books with Dapper 
app.MapGet("/getallbooks", async (MySqlConnection connection) =>
{
    try
    {
        var books = await connection.QueryAsync<Book>("SELECT * FROM BOOKS");
        return Results.Ok(books);
    }
    catch (Exception ex)
    {
        return Results.Problem("failed", ex.Message);
    }
});





app.Run();
