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

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
                return Json(new { success = false, message = "No file selected" });

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, "trainingData.csv");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await csvFile.CopyToAsync(stream);
            }

            // Optionally trigger retraining immediately
            ChatModelTrainer.TrainModel(filePath);

            return Json(new { success = true, message = "CSV uploaded and model retrained!" });
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
