var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddHttpClient(); 
builder.Services.AddEndpointsApiExplorer(); 
 
var app = builder.Build(); 
  
var orders = new List<Order>(); 
  
app.MapGet("/api/orders", () => Results.Ok(orders)); 
app.MapPost("/api/orders", async (OrderDto dto, IHttpClientFactory factory) => { 
    var order = new Order(orders.Count + 1, dto.ProductId, dto.Quantity, 
DateTime.UtcNow); 
    orders.Add(order); 
     
    // Call Notification Service 
    var client = factory.CreateClient(); 
    await client.PostAsJsonAsync( 
        "http://notification-service/api/notifications", 
        new { Message = $"Order {order.Id} created for product {dto.ProductId}" 
} 
    ); 
    return Results.Created($"/api/orders/{order.Id}", order); 
}); 
  
app.Run(); 
record Order(int Id, int ProductId, int Quantity, DateTime CreatedAt); 
record OrderDto(int ProductId, int Quantity);