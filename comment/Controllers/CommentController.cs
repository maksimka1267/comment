using comment.Data.Model;
using comment.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace comment.Controllers
{
    public class CommentController : Controller
    {
        private readonly ICommentRepository _comment;
        public CommentController(ICommentRepository commentRepository)
        {
            _comment = commentRepository;
        }
        [HttpPost]
        public async Task<IActionResult> Add(Comment model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model); // Вернуть форму с ошибками
            }

            model.CreatedAt = DateTime.UtcNow; // Устанавливаем дату
            await _comment.MakeCommentAsync(model); // Добавление в базу

            return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
        }
        [HttpPost]
        public async Task<IActionResult> Reply(Comment model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home"); // Если валидация не прошла, вернемся на главную
            }

            // Проверяем, существует ли родительский комментарий
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

            return RedirectToAction("Index", "Home"); // Обновляем страницу после ответа
        }
        [HttpPost]
        public async Task<IActionResult> Add(Comment model, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index");

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            // Обработка файлов
            var attachments = new List<Attachment>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".txt" };

            foreach (var file in Files)
            {
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension) || file.Length > 100 * 1024)
                    {
                        return BadRequest("Недопустимый файл.");
                    }

                    string fileName = Guid.NewGuid() + extension;
                    string filePath = Path.Combine("wwwroot/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    attachments.Add(new Attachment
                    {
                        Id = Guid.NewGuid(),
                        CommentId = model.Id,
                        FilePath = "/uploads/" + fileName,
                        FileType = extension == ".txt" ? Microsoft.Graph.Models.AttachmentType.File : Microsoft.Graph.Models.AttachmentType.File,
                    });
                }
            }

            model.Attachments = attachments;
            await _comment.MakeCommentAsync(model);

            return RedirectToAction("Index");
        }
    }
}
