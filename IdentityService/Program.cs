var builder = WebApplication.CreateBuilder(args); 
builder.Services.AddEndpointsApiExplorer(); 
var app = builder.Build(); 
  
app.MapPost("/api/identity/register", (UserDto user) => 
    Results.Ok(new { message = "User registered", username = user.Username })); 
  
app.MapPost("/api/identity/login", (UserDto user) => { 
    // INTENTIONAL VULNERABILITY: Hardcoded secret (CWE-798) 
    var secret = "super-secret-key-12345"; 

    var token = $"fake-jwt-{user.Username}-{secret.GetHashCode()}"; 
    return Results.Ok(new { token, message = "Login successful" }); 
}); 
  
app.Run(); 
record UserDto(string Username, string Password);