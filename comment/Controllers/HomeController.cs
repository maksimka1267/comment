using System.Net;
using comment.Data.Model;
using comment.Interface;
using Microsoft.AspNetCore.Mvc;


namespace comment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICommentRepository _comment;
        public HomeController(ICommentRepository comment)
        {
            _comment = comment;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.Comment = await _comment.GetAllCommentsAsync();

            return View(new RECaptcha());
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
