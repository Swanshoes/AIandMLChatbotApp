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
            return Json(result);  
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
                return Json(new { success = false, message = "No file selected" });

            var ext = Path.GetExtension(csvFile.FileName).ToLower();
            if (ext != ".csv")
                return Json(new { success = false, message = "Invalid file type. Please upload a CSV." });

            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            string newFilePath = Path.Combine(folder, "trainingData_new.csv");
            string finalFilePath = Path.Combine(folder, "trainingData.csv");
            string backupFilePath = Path.Combine(folder, $"trainingData_backup_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            try
            {
                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await csvFile.CopyToAsync(stream);
                }

                if (System.IO.File.Exists(finalFilePath))
                    System.IO.File.Copy(finalFilePath, backupFilePath, true);
                try
                {
                    ChatModelTrainer.TrainModel(newFilePath);
                    System.Diagnostics.Debug.WriteLine($"Model trained");

                } 
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Training error: {ex}");

                }

                System.IO.File.Copy(newFilePath, finalFilePath, true);
                System.IO.File.Delete(newFilePath);

                return Json(new { success = true, message = "New dataset uploaded and model retrained successfully." });
            }
            catch (Exception ex)
            {
                if (System.IO.File.Exists(newFilePath))
                    System.IO.File.Delete(newFilePath);

                var latestBackup = Directory.GetFiles(folder, "trainingData_backup_*.csv")
                                            .OrderByDescending(f => f)
                                            .FirstOrDefault();

                if (latestBackup != null)
                {
                    System.IO.File.Copy(latestBackup, finalFilePath, true);
                }

                return Json(new
                {
                    success = false,
                    message = $"Training failed: {ex.Message}. Restored last known good dataset."
                });
            }
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
