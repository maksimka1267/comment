using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace comment.EventHandlers
{
    public class CommentAddedEventHandler : INotificationHandler<comment.Events.CommentAddedEvent>
    {
        public Task Handle(comment.Events.CommentAddedEvent notification, CancellationToken cancellationToken)
        {
            // Здесь можно добавить логику для обработки события, например отправку уведомления
            Console.WriteLine($"Комментарий добавлен: {notification.UserName} ({notification.Email}) в {notification.CreatedAt}");

            // Логика: отправить уведомление, записать в лог, и т.д.
            return Task.CompletedTask;
        }
    }
}
