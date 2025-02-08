using comment.Data.Model;
using comment.Interface;
using comment.Services.Interface;

public class CommentQueueProcessor : BackgroundService
{
    private readonly IQueueService<CommentQueueItem> _queueService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CommentQueueProcessor(
        IQueueService<CommentQueueItem> queueService,
        IServiceScopeFactory serviceScopeFactory)
    {
        _queueService = queueService;
        _serviceScopeFactory = serviceScopeFactory;
    }
    public async Task ProcessCommentAsync(CommentQueueItem queueItem)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var commentRepository = scope.ServiceProvider.GetRequiredService<ICommentRepository>();
            var attachmentRepository = scope.ServiceProvider.GetRequiredService<IAttachmentRepository>();

            // Обработка комментария
            await commentRepository.MakeCommentAsync(queueItem.Comment);

            // Обработка файлов
            if (queueItem.Files.Any())
            {
                var attachments = await ProcessFiles(queueItem.Files, queueItem.Comment.Id, attachmentRepository);
                queueItem.Comment.Attachments = attachments.Select(a => a.Id).ToList();
                await commentRepository.UpdateCommentAsync(queueItem.Comment);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var queueItem = await _queueService.DequeueAsync(stoppingToken);

            if (queueItem != null)
            {
                // Создаем новый scope для работы с scoped-сервисами
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var commentRepository = scope.ServiceProvider.GetRequiredService<ICommentRepository>();
                    var attachmentRepository = scope.ServiceProvider.GetRequiredService<IAttachmentRepository>();

                    // Обработка комментария
                    await commentRepository.MakeCommentAsync(queueItem.Comment);

                    // Обработка файлов
                    if (queueItem.Files.Any())
                    {
                        var attachments = await ProcessFiles(queueItem.Files, queueItem.Comment.Id, attachmentRepository);
                        queueItem.Comment.Attachments = attachments.Select(a => a.Id).ToList();
                        await commentRepository.UpdateCommentAsync(queueItem.Comment);
                    }
                }
            }
        }
    }
    private async Task<List<Attachment>> ProcessFiles(List<(byte[] FileData, string FileName)> files, Guid commentId, IAttachmentRepository attachmentRepository)
    {
        var attachments = new List<Attachment>();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".txt" };

        foreach (var (fileData, fileName) in files)
        {
            var extension = Path.GetExtension(fileName).ToLower();

            if (!allowedExtensions.Contains(extension) || fileData.Length > 100 * 1024)
            {
                throw new InvalidOperationException("Недопустимый файл.");
            }

            AttachmentType fileType = (extension == ".txt") ? AttachmentType.File : AttachmentType.Image;

            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                FileData = fileData, // Используем уже готовый массив байтов
                FileType = fileType
            };

            attachments.Add(attachment);
            await attachmentRepository.AddAsync(attachment);
        }

        return attachments;
    }

}
