using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MessageLengthBot.Controllers;

public class TextMessageController(ITelegramBotClient botClient, ILoggerFactory loggerFactory)
{
    /// <summary>
    /// Определеяем объект логирования
    /// </summary>
    private readonly ILogger<TextMessageController>? _logger = loggerFactory.CreateLogger<TextMessageController>();

    public async Task Handle(Message message, CancellationToken stoppingToken)
    {
        _logger?.LogInformation(
            "От пользователя {UserName} получено текстовое сообщение: {TextMessage}", 
            message.From?.Username ?? "<НЕИЗВЕСТНОЮ>", 
            message.Text);

        switch (message.Text)
        {
            case "/start":

                var buttons = new List<InlineKeyboardButton[]>
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Счетчик символов", "char_counter"),
                        InlineKeyboardButton.WithCallbackData("Сумма чисел", "sum_numbers")
                    }
                };

                await botClient.SendMessage
                (
                    chatId: message.Chat.Id,
                    cancellationToken: stoppingToken,
                    parseMode: ParseMode.Html,
                    replyMarkup: new InlineKeyboardMarkup(buttons),
                    text:
                    $"<b>Этот бот может обрабатывать строки текста</b> {Environment.NewLine}" +
                    ""
                    )
        };
        }
    }
}