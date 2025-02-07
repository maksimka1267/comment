using comment.Data.Model;
using comment.Interface;
using comment.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace comment.Controllers
{
    [Route("Comment")]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _comment;
        private readonly IAttachmentRepository _attachment;
        public CommentController(ICommentRepository commentRepository, IAttachmentRepository attachment)
        {
            _comment = commentRepository;
            _attachment = attachment;
        }

        // Вспомогательный метод для обработки файлов
        private async Task<List<Attachment>> ProcessFiles(List<IFormFile> files, Guid commentId)
        {
            var attachments = new List<Attachment>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".txt" };

            foreach (var file in files)
            {
                if (file != null)
                {
                    var extension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension) || file.Length > 100 * 1024)
                    {
                        throw new InvalidOperationException("Недопустимый файл.");
                    }

                    // Читаем содержимое файла в массив байтов
                    byte[] fileBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }

                    // Присваиваем правильный тип (Image или File)
                    AttachmentType fileType = (extension == ".txt") ? AttachmentType.File : AttachmentType.Image;

                    var attachment = new Attachment
                    {
                        Id = Guid.NewGuid(),
                        CommentId = commentId,
                        FileData = fileBytes, // Сохраняем данные в виде массива байтов
                        FileType = fileType
                    };

                    attachments.Add(attachment);

                    // Сохраняем attachment в базе данных
                    await _attachment.AddAsync(attachment);
                }
            }

            return attachments;
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(Comment model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            model.CreatedAt = DateTime.UtcNow;
            await _comment.MakeCommentAsync(model);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("AddWithFile")]
        public async Task<IActionResult> AddWithFile(Comment model, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            await _comment.MakeCommentAsync(model);

            var attachments = await ProcessFiles(Files, model.Id);

            model.Attachments = attachments.Select(a => a.Id).ToList();
            await _comment.UpdateCommentAsync(model);

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
        public async Task<IActionResult> ReplyWithFile(Comment model, List<IFormFile> Files)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index","Home");

            if (model.ParentId != null)
            {
                var parentComment = await _comment.GetCommentByIdAsync(model.ParentId.Value);
                if (parentComment == null)
                {
                    return NotFound("Родительский комментарий не найден.");
                }
            }

            model.Id = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            await _comment.MakeCommentAsync(model);

            var attachments = await ProcessFiles(Files, model.Id);

            model.Attachments = attachments.Select(a => a.Id).ToList();
            await _comment.UpdateCommentAsync(model);

            return RedirectToAction("Index","Home");
        }
    }
}
