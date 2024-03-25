using ANXRacers.Serializables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host
.CreateDefaultBuilder(args)
.ConfigureServices((builder, services)=>{
    Console.WriteLine($"Configuring Services");
    services.Configure<Configs>(builder.Configuration.GetRequiredSection("Configs"));
})
.RunConsoleAsync();

