var builder = WebApplication.CreateBuilder(args);

// 1. Add Controller Support
builder.Services.AddControllers();

// 2. Enable CORS so your React app (port 5173) can talk to this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("VitePolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var app = builder.Build();

// 3. Use the CORS policy
app.UseCors("VitePolicy");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();


app.Run();