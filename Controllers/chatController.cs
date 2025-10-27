using Microsoft.AspNetCore.Mvc;
using AIandMLChatbotApp.Models;
using System.Text.Json;

namespace AIandMLChatbotApp.Controllers
{
    public class ChatController : Controller
    {
        private static List<QuestionAnswer> knowledgeBase = new List<QuestionAnswer>
        {
            new QuestionAnswer { Question = "What is AI?", Answer = "AI stands for Artificial Intelligence." },
            new QuestionAnswer { Question = "What is ML?", Answer = "ML stands for Machine Learning." },
            new QuestionAnswer { Question = "What is a chatbot?", Answer = "A chatbot is a software application used to conduct an online chat conversation via text or text-to-speech." }
        };

        public IActionResult Ask([FromBody] JsonElement request)
        {
            string userQuestion = request.GetProperty("question").GetString();

            var answer = knowledgeBase.FirstOrDefault(kb => userQuestion.ToLower().Contains(kb.Question.ToLower()));
            if (answer != null)
                            {
                return Json(new { answer = answer.Answer });
            }
            else
            {
                return Json(new { answer = "I'm sorry, I don't have an answer for that question." });
            }
        }
        
    }
}
