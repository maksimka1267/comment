using comment.Data.Model;
using comment.Services.Interface;

public class QueueService<T> : IQueueService<T>
{
    private readonly Queue<CommentQueueItem> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Enqueue(CommentQueueItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_queue)
        {
            _queue.Enqueue(item);
        }

        _signal.Release();
    }

    public async Task<CommentQueueItem> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);

        lock (_queue)
        {
            return _queue.Dequeue();
        }
    }
}
