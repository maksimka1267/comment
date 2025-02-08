using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class WebSocketHandler
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new ConcurrentDictionary<string, WebSocket>();

    public async Task HandleConnections(HttpContext context)
    {
        if (context.WebSockets.IsWebSocketRequest) // Проверка на WebSocket запрос
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();  // Принимаем соединение WebSocket
            var connectionId = context.Connection.Id; // Уникальный идентификатор для соединения

            _connections.TryAdd(connectionId, webSocket); // Сохраняем соединение

            await ReceiveMessages(webSocket, connectionId); // Ожидаем сообщения от клиента
        }
        else
        {
            context.Response.StatusCode = 400;  // Возвращаем ошибку, если запрос не WebSocket
        }
    }

    private async Task ReceiveMessages(WebSocket webSocket, string connectionId)
    {
        var buffer = new byte[1024 * 4];  // Буфер для чтения данных
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);  // Получаем данные

        while (!result.CloseStatus.HasValue) // Пока соединение открыто
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                // Логика для обработки полученного сообщения
                var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Можешь добавить логику для обработки и отправки сообщений

                // Например, отправка того же сообщения обратно
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
            }

            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);  // Ожидаем следующее сообщение
        }

        _connections.TryRemove(connectionId, out _);  // Удаляем соединение из списка после закрытия
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);  // Закрытие соединения
    }
    public async Task SendToAllAsync(string message)
    {
        foreach (var webSocket in _connections.Values)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

}
