using System.Data.Common;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using BookTrackerApi.Data;          // Finds ItemDbContext
using Microsoft.EntityFrameworkCore; // Finds UseSqlServer
var builder = WebApplication.CreateBuilder(args);


// In Program.cs
builder.Services.AddDbContext<ItemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

app.MapGet("/",() => "hello world");

//Create new item
app.MapGet("/items", async (ItemDbContext db) =>
{
 await db.ReadItems.ToListAsync();
    await db.ReadItems.ToAsync();
});

app.MapPost("items", async( ItemDbContext db, ReadItem item)=>
{
    /*db (The ItemDbContext): You nailed it. It is the object that holds the configuration. It knows where the database is (your Connection String) and how the tables are structured (your DbSet properties). It is the gateway to the database.

db.ReadItems (The DbSet): This is the property that acts as the "handle" for the table. It tells EF Core: "I want to work with the 'ReadItems' table."*/

    db.ReadItems.ToListAsync(item);//Basically SELECTS the rows from the db and align them with the ones inside the class 
    await db.SaveChangesAsync();//syncs to the database
    return Results.Created($"/items/{item.Id}", item);

});

app.MapDelete("/items{id}", async(ItemDbContext db, int id ) =>
{
    var item = await db.ReadItems.FindAsync(id);
    if (item is null) 
        return Results.NotFound();
    
    db.ReadItems.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();    
});

app.MapGet("/items/{id}", async(ItemDbContext db, int id) =>
{
    var item = await db.ReadItems.FindAsync(id);
    return item is not null ? 
    Results.ok(item) :Results.NotFound();
});

//PUT
app.MapPut("/items/{id}", async (ItemDbContext db, int id, ReadItem updatedItem) =>
{
    var item = await db.ReadItems.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = updatedItem.Name;
    item.Description = updatedItem.Description;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.Run();
