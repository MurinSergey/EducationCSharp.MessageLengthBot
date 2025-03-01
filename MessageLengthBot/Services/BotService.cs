using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MessageLengthBot.Services;

public class BotService : BackgroundService
{
    
    /// <summary>
    /// Экземпляр для вывода информации о процессе
    /// </summary>
    private readonly ILogger<BotService>? _logger;
    
    /// <summary>
    /// Экземпляр бота
    /// </summary>
    private readonly ITelegramBotClient _botClient;

    private const string NoText = "<нет текста>";
    private const string NoUserName = "<неизвестный>";

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="logger">Экземпляр для вывода информации о процессе</param>
    /// <param name="botClient">Экземпляр бота</param>
    public BotService(ILogger<BotService> logger, ITelegramBotClient botClient)
    {
        _logger = logger;
        _botClient = botClient;
    }
    
    /// <summary>
    /// Запуск бота как сервис
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions(){AllowedUpdates = []},
            cancellationToken: stoppingToken
            );

        _logger?.LogInformation("Бот запущен");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException ex)
            {
                _logger?.LogWarning(ex, "Бот остановлен принудительно");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Бот остановлен по ошибке");
            }
        }
    }
    
    /// <summary>
    /// Обработка событий бота
    /// </summary>
    /// <param name="botClient">Экземпляр бота</param>
    /// <param name="update">Информация от бота</param>
    /// <param name="stoppingToken">Метка отмены задачи</param>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient , Update update, CancellationToken stoppingToken)
    {
        switch (update)
        {
            case { Type: UpdateType.Message, Message: not null }:
                var message = update.Message;
                var textMsg = message.Text ?? NoText;
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"В вашем сообщении: {textMsg.Length} символов",
                    cancellationToken: stoppingToken
                    );
                _logger?.LogInformation("Пользователь {Username} прислал сообщение длинной {Length}", message.From?.Username ?? NoUserName, textMsg.Length);
                return;
            case { Type: UpdateType.CallbackQuery, CallbackQuery: not null, CallbackQuery.Message: not null}:
                var callBack = update.CallbackQuery;
                var textBtn = callBack.Message.Text ?? NoText;
                await botClient.SendMessage(
                    chatId: callBack.Message.Chat.Id,
                    text: $"Вы нажали кнопку с текстом: {textBtn}",
                    cancellationToken: stoppingToken
                    );
                _logger?.LogInformation("Пользователь {Username} использовал кнопку с текстом {Length}", callBack.Message.From?.Username ?? NoUserName, textBtn);
                return;
        }
    }

    /// <summary>
    /// Обработка ошибок
    /// </summary>
    /// <param name="botClient">Экземпляр бота</param>
    /// <param name="exception">Информация от бота</param>
    /// <param name="stoppingToken">Метка отмены задачи</param>
    private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken stoppingToken)
    {
        switch (exception)
        {
            case ApiRequestException apiRequestException:
                _logger?.LogError(apiRequestException, "Ошибка Telegram API");
                break;
            default:
                _logger?.LogError(exception, "Что-то не так...");
                break;
        }

        try
        {
            _logger?.LogInformation("Повторное подключение через 10 секунд");
            await Task.Delay(10000, stoppingToken);
        }
        catch (OperationCanceledException ex)
        {
            _logger?.LogWarning(ex, "Принудительное подключение");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка переподключения");
        }
    }
}