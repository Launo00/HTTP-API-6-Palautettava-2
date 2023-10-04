using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BooksDb>(opt => opt.UseInMemoryDatabase("BookList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/Books", async (BooksDb db) =>
    await db.Books.ToListAsync());

app.MapGet("/Books/{id}", async (int id, BooksDb db) =>
    await db.Books.FindAsync(id)
        is Books books
            ? Results.Ok(books)
            : Results.NotFound());

app.MapPost("/Books", async (Books books, BooksDb db) =>
{
    db.Books.Add(books);
    await db.SaveChangesAsync();

    return Results.Created($"/Books/{books.Id}", books);
});

app.MapPut("/Books/{id}", async (int id, Books inputBook, BooksDb db) =>
{
    var books = await db.Books.FindAsync(id);

    if (books is null) return Results.NotFound();

    books.Name = inputBook.Name;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/Books/{id}", async (int id, BooksDb db) =>
{
    if (await db.Books.FindAsync(id) is Books books)
    {
        db.Books.Remove(books);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();

public class Books
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

class BooksDb : DbContext
{
    public BooksDb(DbContextOptions<BooksDb> options)
        : base(options) { }

    public DbSet<Books> Books => Set<Books>();
}