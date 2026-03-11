var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddEndpointsApiExplorer(); 
var app = builder.Build(); 
  
var products = new List<Product> { 
    new(1, "Laptop", 999.99m), 
    new(2, "Phone", 699.99m) 
}; 
  
app.MapGet("/api/products", () => Results.Ok(products)); 
app.MapGet("/api/products/{id}", (int id) => { 
    var p = products.FirstOrDefault(p => p.Id == id); 
    return p is not null ? Results.Ok(p) : Results.NotFound(); 
}); 
app.MapPost("/api/products", (Product product) => { 
    products.Add(product); 
    return Results.Created($"/api/products/{product.Id}", product); 
}); 
app.MapDelete("/api/products/{id}", (int id) => { 
    var p = products.FirstOrDefault(p => p.Id == id); 
    if (p is null) return Results.NotFound(); 
    products.Remove(p); 
    return Results.NoContent(); 
}); 
  
app.Run(); 
record Product(int Id, string Name, decimal Price); 