
using MessageLengthBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

// Фабрика логеров
using var loggerFactory = LoggerFactory.Create(
    builder =>
    {
        builder.AddConsole(); // подключаем вывод в консоль
    });

var botLogger = loggerFactory.CreateLogger<BotService>();
var mainLogger = loggerFactory.CreateLogger<Program>();

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("botconfig.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostingContext, services) => ConfigureServices(services, hostingContext.Configuration, botLogger))
    .Build();

mainLogger.LogInformation("Запуск сервиса");

await host.RunAsync();

mainLogger.LogInformation("Сервис остановлен");


return;

static void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger<BotService> botLogger)
{
    var token = configuration["TelegramToken"];
    
    services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));
    services.AddSingleton(botLogger);
    services.AddHostedService<BotService>();
}
    