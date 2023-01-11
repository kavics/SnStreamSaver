using Microsoft.Extensions.Configuration;
using SnStreamSaver;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets("f8c73f38-2fc8-4572-851a-88df5bc81a0c")
    .Build();

var app = new App(config, args);
await app.RunAsync(CancellationToken.None);