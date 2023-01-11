using Microsoft.Extensions.Configuration;
using SnStreamSaver;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
    .Build();

var app = new App(config, args);
await app.RunAsync(CancellationToken.None);