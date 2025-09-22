using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;

namespace DotNetCoreAppPostgres.Controllers
{
    public class AIPlaygroundController : Controller
    {
        private readonly ChatClient _chatClient;

        public AIPlaygroundController(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        [HttpGet]
        public IActionResult Playground()
        {
            return View("AIPlayground");
        }

        [HttpPost]
        public async Task<IActionResult> Playground(string actionType, string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                ViewBag.Result = "⚠️ Please enter something first!";
                return View("AIPlayground");
            }

            string prompt = actionType switch
            {
                "joke" => $"Tell me a short and funny joke about {userInput}.",
                "story" => $"Write a short, fun story about: {userInput}. Limit to one paragraph.",
                "poem" => $"Write a short creative poem about {userInput}.",
                _ => $"Just say something fun about {userInput}."
            };

            var response = await _chatClient.CompleteChatAsync(new[]
            {
                new UserChatMessage(prompt)
            });

            ViewBag.Result = response.Value.Content[0].Text;
            ViewBag.SelectedAction = actionType;
            ViewBag.UserInput = userInput;

            return View("AIPlayground");
        }
    }
}
