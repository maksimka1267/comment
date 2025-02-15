﻿using System.Net;
using comment.Data.Model;
using comment.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;


namespace comment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICommentRepository _comment;
        private readonly IAttachmentRepository _attachment;
        private readonly IMemoryCache _cache;
        public HomeController(ICommentRepository comment, IAttachmentRepository attachment, IMemoryCache cache)
        {
            _comment = comment;
            _attachment = attachment;
            _cache = cache;
        }
        public async Task<IActionResult> Index()
        {
            var cacheKey = "all_comments";
            if (!_cache.TryGetValue(cacheKey, out List<Comment> comments))
            {
                // Данные не найдены в кэше, делаем запрос к базе
                comments = await _comment.GetAllCommentsAsync();

                // Устанавливаем кэш на 1 минуту
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            ViewBag.Comment = comments;
            return View(new RECaptcha());
        }
        [HttpGet]
        public async Task<IActionResult> GetImage(Guid id)
        {
            var imageData = await _attachment.GetByIdAsync(id);

            if (imageData == null)
            {
                return NotFound(); // Если изображения нет, возвращаем 404
            }

            return File(imageData.FileData, "image/jpeg"); // Отправляем изображение клиенту
        }
        [HttpPost]
        public JsonResult AjaxMethod(string response)
        {
            RECaptcha recaptcha = new RECaptcha();
            string url = "https://www.google.com/recaptcha/api/siteverify?secret=" + recaptcha.Secret + "&response=" + response;
            recaptcha.Response = new WebClient().DownloadString(url);
            return Json(recaptcha);
        }
    }
}
