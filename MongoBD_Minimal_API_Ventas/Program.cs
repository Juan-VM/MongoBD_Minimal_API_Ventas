using MongoBD_Minimal_API_Ventas.Endpoint;
using MongoDB.Driver;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = "mongodb+srv://userName:Password@cluster0.fd4u3wk.mongodb.net/";

builder.Services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.MapVentasEndpoint();


app.Run();

