using System.Text.Json;
using ForaChallenge.Api;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IEdgarService, EdgarService>(
    serviceProvider => new EdgarService(
        httpClient: serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ForaChallenge"),
        logger: serviceProvider.GetRequiredService<ILogger<EdgarService>>(),
        maxParallelRequests: Convert.ToInt32(builder.Configuration["MaxParallelEdgarRequests"])));
builder.Services.AddHttpClient<IEdgarService, EdgarService>(client => 
{
    client.DefaultRequestHeaders.Add("User-Agent", "PostmanRuntime/7.34.0");
    client.DefaultRequestHeaders.Add("Accept", "*/*");
});

builder.Services.AddControllers();
builder.Services
    .Configure<JsonOptions>(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.WriteIndented = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();