using Microsoft.Extensions.Configuration;
using SnStreamSaver;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
    .Build();

new App(config, args).Run();