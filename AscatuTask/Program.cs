using AscatuTask;
using AscatuTask.Data;
using AscatuTask.Services;
using BillingManagementWebApp.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));
builder.Services.AddScoped<IOrderDbContext,OrderDbContext>();
builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();
builder.Services.AddSingleton<ILoggerService, LoggerService>();

builder.Services.AddHostedService<KafkaConsumerService>();

builder.Services.AddHttpClient();




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Order Service API",
        Description = "API that allows to perform CRUD operations for order information stored in a DB.",
    }
    );
    options.AddServer(new OpenApiServer
    {
        Url = "http://localhost:8081",
        Description = "Inferred Url"
    });

});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DataGenerator.Initialize(services);
}

    app.UseSwagger();
    app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseCustomExceptionMiddleware();

app.Urls.Add("http://+:8080");


app.MapGet("/api/v1/order", async (IOrderDbContext db) => { return await db.Orders.ToListAsync(); })
    .WithName("GetOrders").WithDescription("Get all orders.").WithDisplayName("GetOrders").WithSummary("Get all orders.")
    .WithOpenApi();

app.MapGet("/api/v1/order/{id}", async (string id, IOrderDbContext db) => { return await db.Orders.FindAsync(id); })
    .WithName("GetOrder").WithSummary("Get order by id.").WithDescription("Get order by id.").WithDisplayName("GetOrder")
    .WithOpenApi();

app.MapPost("/api/v1/order", async (OrderViewModel orderVM, IOrderDbContext db, IKafkaProducer kafkaProducer, HttpClient http) =>
{
    if (orderVM.Quantity <= 0) return Results.BadRequest("Quantity must be greater than 0.");
    if(string.IsNullOrWhiteSpace(orderVM.Product)) return Results.BadRequest("Product name is required.");
    if(string.IsNullOrEmpty(orderVM.PersonId)) return Results.BadRequest("PersonId is required.");

    await Util.CheckPersonIdAsync(http, orderVM.PersonId);

    var order = new Order
    {
        PersonId = orderVM.PersonId,
        Product = orderVM.Product,
        Quantity = orderVM.Quantity,
        Date = DateTime.Now
    };
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    await kafkaProducer.ProduceAsync("orderevents-created", order, "created");

    return Results.Created($"/orders/{order.Id}", order);
}).WithName("CreateOrder").WithDescription("Create a new order.").WithDisplayName("CreateOrder").WithSummary("Create a new order.").WithOpenApi();

app.MapPut("/api/v1/order/{id}", async (string id, OrderViewModel updatedOrder, IOrderDbContext db, IKafkaProducer kafkaProducer, HttpClient http) =>
{
    if (updatedOrder.Quantity <= 0) return Results.BadRequest("Quantity must be greater than 0.");
    if (string.IsNullOrWhiteSpace(updatedOrder.Product)) return Results.BadRequest("Product name is required.");
    if (string.IsNullOrEmpty(updatedOrder.PersonId)) return Results.BadRequest("PersonId is required.");

    var order = await db.Orders.FindAsync(id);
    if (order == null) return Results.NotFound();

    if(order.PersonId != updatedOrder.PersonId)
    {
        await Util.CheckPersonIdAsync(http, updatedOrder.PersonId);
    }

    if (order.PersonId != updatedOrder.PersonId || order.Quantity != updatedOrder.Quantity || order.Product != updatedOrder.Product)
    {
        order.PersonId = updatedOrder.PersonId;
        order.Product = updatedOrder.Product;
        order.Quantity = updatedOrder.Quantity;
        await db.SaveChangesAsync();
    }

    await kafkaProducer.ProduceAsync("orderevents-updated", order, "updated");

    return Results.Ok(order);
}).WithName("UpdateOrder").WithDescription("Update an existing order.").WithDisplayName("UpdateOrder").WithSummary("Update an existing order.").WithOpenApi();

app.MapDelete("/api/v1/order/{id}", async (string id, IOrderDbContext db, IKafkaProducer kafkaProducer) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order == null) return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();

    await kafkaProducer.ProduceAsync("orderevents-deleted", order, "deleted");

    return Results.NoContent();
}).WithName("DeleteOrder").WithDescription("Delete an existing order.").WithDisplayName("DeleteOrder").WithSummary("Delete an existing order.").WithOpenApi();

app.MapDelete("/api/v1/order", async (IOrderDbContext db, IKafkaProducer kafkaProducer) =>
{
    var orders = await db.Orders.ToListAsync();
    db.Orders.RemoveRange(orders);
    await db.SaveChangesAsync();

    foreach (var order in orders)
    {
        await kafkaProducer.ProduceAsync("orderevents-deleted", order, "deleted");
    }

    return Results.NoContent();
}).WithName("DeleteAllOrders").WithDescription("Delete all orders.").WithDisplayName("DeleteAllOrders").WithSummary("Delete all orders.").WithOpenApi();

app.Run();