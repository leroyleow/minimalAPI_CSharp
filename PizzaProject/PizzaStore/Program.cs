using Microsoft.OpenApi.Models;
using PizzaStore.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";
//The builder has a Services property. By using the Services property, you can add features like CORS, Entity Framework, or Swagger.
//E.g. builder.Services.AddCors(options => {});

builder.Services.AddEndpointsApiExplorer();
//Use in memory
//builder.Services.AddDbContext<PizzaDB>(options => options.UseInMemoryDatabase("items"));
//Use SQL
builder.Services.AddSqlite<PizzaDB>(connectionString);
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo{ Title = "PizzaStore Api", Description="Making the Pizza you love", Version = "v1"});
});

//CORS start
// 1) define a unique string
readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 2) define allowed domains, in this case "http://example.com" and "*" = all
//    domains, for testing purposes only.
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
      builder =>
      {
          builder.WithOrigins(
            "http://example.com", "*");
      });
});


var app = builder.Build();   


app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});
// 3) use the capability
app.UseCors(MyAllowSpecificOrigins);

app.MapGet("/", () => "Hello World!");
// app.MapGet("/pizzas/{id}", (int id) => PizzaDB.GetPizza(id));
// app.MapGet("/pizzas", ()=> PizzaDB.GetPizzas());
// app.MapPost("/pizzas", (Pizza pizza) => PizzaDB.CreatePizza(pizza));
// app.MapPut("/pizzas", (Pizza pizza) => PizzaDB.UpdatePizza(pizza));
// app.MapDelete("/pizzas/{id}", (int id) => PizzaDB.RemovePizza(id));
app.MapGet("/pizzas", async (PizzaDB db) => await db.Pizzas.ToListAsync());
app.MapPost("/pizza", async (PizzaDB db, Pizza pizza) =>{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});
app.MapGet("/pizza/{id}", async (PizzaDB db, int id) => await db.Pizzas.FindAsync(id));
app.MapPut("/pizza/{id}", async (PizzaDB db, Pizza updatepizza, int id) => 
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatepizza.Name;
    pizza.Description = updatepizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/pizza/{id}", async (PizzaDB db, int id) => {
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null){
        return Results.NotFound();
    }
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});


app.Run();
