using comment.Data.Model;
using comment.Interface;
using comment.Repository;
using comment.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace comment.Controllers
{
    [Route("Comment")]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _comment;
        private readonly IAttachmentRepository _attachment;
        private readonly IQueueService<CommentQueueItem> _commentQueue;

        public CommentController(ICommentRepository commentRepository, IAttachmentRepository attachment, IQueueService<CommentQueueItem> commentQueue)
        {
            _comment = commentRepository;
            _attachment = attachment;
            _commentQueue = commentQueue; // Инъекция очереди
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

            // Собираем файлы в массив байтов сразу
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
                Files = fileDataList // Передаем массив байтов вместо IFormFile
            });

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

            // Собираем файлы в массив байтов сразу
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
                Files = fileDataList // Передаем массив байтов вместо IFormFile
            });


            return RedirectToAction("Index", "Home");
        }

    }
}
