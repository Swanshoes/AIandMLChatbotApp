using Microsoft.AspNetCore.Mvc;
using AIandMLChatbotApp.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace AIandMLChatbotApp.Controllers
{
    public class ChatController : Controller
    {
        [HttpPost]
        public IActionResult Ask([FromBody] JsonElement request)
        {
            string userQuestion = request.GetProperty("question").GetString();
            ChatResponse result = ChatModelTrainer.Predict(userQuestion);
            return Json(result);  // returns both Answer and Confidence
        }

        public IActionResult Train()
        {
            ChatModelTrainer.TrainModel();
            return Content("Model trained and saved!");
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
