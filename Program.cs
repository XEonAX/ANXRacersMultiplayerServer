using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.Configure<Configs>(builder.Configuration.GetRequiredSection("Configs"));
builder.Services.AddSingleton<IStateService, StateService>();
builder.Services.AddSingleton<Communications>();
builder.Services.AddHostedService(provider => provider.GetService<Communications>());//Host the singleton instance
builder.Services.AddHttpClient<Communications>();
builder.Services.AddSingleton<IMultiplayerServer, MultiplayerServer>();
builder.Services.AddHostedService<ServerHost>();

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
