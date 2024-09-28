using Microsoft.EntityFrameworkCore;
using Dapper;
using MySqlConnector;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Text.Json.Serialization;


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

//extract with id 
app.MapGet("/getjsondetail/{id}", async (MySqlConnection connection, int id) =>
{
    try
    {
        var aBook = await connection.QueryAsync<Book>($"SELECT JSON_EXTRACT(attributes,'$.*') As Attributes FROM Books WHERE id=@id", new { id });
        return Results.Ok(aBook);
    }
    catch (Exception ex)
    {
        return Results.Problem("failed", ex.Message);
    }

});

app.MapGet("/getonepet/{id}", async (MySqlConnection connection, int id) =>
{
    try
    {
        var pet = await connection.QueryAsync<Pet>($"SELECT * FROM pets WHERE id=@id", new { id });
        return Results.Ok(pet);
    }
    catch (Exception ex)
    {
        return Results.Problem("Failes", ex.Message);
    }


}


);

//update json part of an id 
app.MapPut("/updatepet/{id}", async (MySqlConnection connection, int id) =>
{
    try
    {
        var query = @"UPDATE pets 
                    SET attributes = JSON_SET(attributes, '$.color', 'red and blue')
                    WHERE id = @id";


        var rowsAffected = await connection.ExecuteAsync(query, new { id });
        return Results.Ok($"Updated {rowsAffected} row(s).");


    }
    catch (Exception ex)
    {
        return Results.Problem("Failes", ex.Message);
    }
});

//add new data to an existing JSON column 

app.MapPut("/newfeature/{id}", async (MySqlConnection connection, int id) =>
{

    try
    {

        var doctor = "Joe Vet";
        var lastVaccine = "rabbies 2024";

        var query = @"UPDATE pets 
                    SET attributes=JSON_SET(attributes, '$.doctor',@doctor, '$.lastVaccine', @vaccine) WHERE id=@id";

        var parameters = new { doctor, vaccine = lastVaccine, id };
        var rowsAffected = await connection.ExecuteAsync(query, parameters);
        return Results.Ok($"Updated {rowsAffected} rows");


    }
    catch (Exception ex)
    {
        return Results.Problem("faile", ex.Message);
    }



});








app.Run();
