using comment.Data.Model;

namespace comment.Services.Interface
{
    public interface IQueueService<T>
    {
        void Enqueue(CommentQueueItem item);
        Task<CommentQueueItem> DequeueAsync(CancellationToken cancellationToken);
    }

}
