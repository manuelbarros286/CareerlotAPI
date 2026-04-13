using CareerlotAPI.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();


// CORS policies
builder.Services.AddCors(options =>
{
    options.AddPolicy("VitePolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://careerlot.vercel.app")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

app.UseCors("VitePolicy");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
} 
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();


app.Run();