using comment.Data.Model;
using comment.Interface;
using comment.Repository;
using comment.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Подключаем кэш

namespace comment.Controllers
{
    [Route("Comment")]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _comment;
        private readonly IAttachmentRepository _attachment;
        private readonly IQueueService<CommentQueueItem> _commentQueue;
        private readonly IMemoryCache _cache; // Добавляем кэш

        public CommentController(ICommentRepository commentRepository, IAttachmentRepository attachment, IQueueService<CommentQueueItem> commentQueue, IMemoryCache cache)
        {
            _comment = commentRepository;
            _attachment = attachment;
            _commentQueue = commentQueue; // Инъекция очереди
            _cache = cache; // Инъекция кэша
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(Comment model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Ошибка валидации" });
            }

            model.CreatedAt = DateTime.UtcNow;
            await _comment.MakeCommentAsync(model);

            // Удаляем кэш
            var cacheKey = "all_comments";
            _cache.Remove(cacheKey);

            // Возвращаем данные нового комментария
            return Json(new { success = true, comment = model });
        }

        [HttpPost("AddWithFile")]
        public IActionResult AddWithFile(Comment model, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            // Собираем файлы в массив байтов
            var fileDataList = new List<(byte[] FileData, string FileName)>();

            foreach (var file in Files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    fileDataList.Add((memoryStream.ToArray(), file.FileName));
                }
            }

            // Передаем CommentQueueItem в очередь
            _commentQueue.Enqueue(new CommentQueueItem
            {
                Comment = model,
                Files = fileDataList
            });

            // Удаляем кэш
            var cacheKey = "all_comments";
            _cache.Remove(cacheKey);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("reply")]
        public async Task<IActionResult> Reply(Comment model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            if (model.ParentId != null)
            {
                var parentComment = await _comment.GetCommentByIdAsync(model.ParentId.Value);
                if (parentComment == null)
                {
                    return NotFound("Родительский комментарий не найден.");
                }
            }

            model.CreatedAt = DateTime.UtcNow;
            await _comment.MakeCommentAsync(model);

            // Удаляем кэш
            var cacheKey = "all_comments";
            _cache.Remove(cacheKey);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("ReplyWithFile")]
        public IActionResult ReplyWithFile(Comment model, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            if (model.ParentId != null)
            {
                var parentComment = _comment.GetCommentByIdAsync(model.ParentId.Value).Result;
                if (parentComment == null)
                {
                    return NotFound("Родительский комментарий не найден.");
                }
            }

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            // Собираем файлы в массив байтов
            var fileDataList = new List<(byte[] FileData, string FileName)>();

            foreach (var file in Files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    fileDataList.Add((memoryStream.ToArray(), file.FileName));
                }
            }

            // Передаем CommentQueueItem в очередь
            _commentQueue.Enqueue(new CommentQueueItem
            {
                Comment = model,
                Files = fileDataList
            });

            // Удаляем кэш
            var cacheKey = "all_comments";
            _cache.Remove(cacheKey);

            return RedirectToAction("Index", "Home");
        }
    }
}
