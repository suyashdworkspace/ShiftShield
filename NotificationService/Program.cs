var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddEndpointsApiExplorer(); 
var app = builder.Build(); 
  
var notifications = new List<string>(); 
  
app.MapGet("/api/notifications", () => Results.Ok(notifications)); 
app.MapPost("/api/notifications", (NotificationDto dto) => { 
    notifications.Add($"[{DateTime.UtcNow:HH:mm:ss}] {dto.Message}"); 
    return Results.Ok(new { message = "Notification logged" }); 
}); 
  
app.Run(); 
record NotificationDto(string Message);