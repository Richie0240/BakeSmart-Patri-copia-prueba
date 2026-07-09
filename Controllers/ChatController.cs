using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace BakeSmartPatri.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _http = httpClientFactory.CreateClient();
            _config = config;
        }

        public record ChatRequest(string Message);

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest req)
        {
            var apiKey = _config["Groq:ApiKey"];

            var body = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                new { role = "system", content = @" Sos Richie, el asistente virtual de BakeSmart Patri, una pastelería/repostería en Costa Rica.
                Hablás en tono cercano, cálido y con un toque tico, pero profesional.
                Te presentás como Richie la primera vez que alguien te escribe.
                No inventés precios ni productos específicos si no te los dan en el contexto.
                Si te preguntan algo que no sabés, decí que van a poder revisar el catálogo en la web o contactar a la tienda.
                Mantené las respuestas cortas, máximo 3-4 líneas.
                " },
                new { role = "user", content = req.Message }
            }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var reply = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Ok(new { reply });
        }
    }
}