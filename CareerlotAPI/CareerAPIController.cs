using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
[ApiController]
[Route("api/[controller]")]
public class CareerAPIController : ControllerBase 
{
    public class CareerMatch
    {
        public string Title { get; set; }
        public string Desc { get; set; }
    }

    public class CvRequest
    {
        public string CvText { get; set; }
    }
    
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public CareerAPIController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _configuration = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("analyse")]
    public async Task<IActionResult> Analyse([FromBody] CvRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.CvText))
        {
            return BadRequest("CV text is required.");
        }
        
        var apiKey = _configuration["OpenRouter:ApiKey"];
        var url = "https://openrouter.ai/api/v1/chat/completions";
        
        var requestBody = new
        {
            model= "liquid/lfm-2.5-1.2b-instruct:free",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content= $"Analyse the following CV text and provide potential job matches and suggest realistic career pivots. Return ONLY a JSON array with 'title' and 'desc' keys: \n\n{request.CvText}"
                }
            },
        };

        var client =  _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        var response = await client.PostAsJsonAsync(url, requestBody);
        
        if (!response.IsSuccessStatusCode) 
        {
            var errorDetails = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, errorDetails);
        }        
        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        var aiTextParse = jsonResponse.GetProperty("choices")
                                 .EnumerateArray()
                                 .First()
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();
        var cleanJson = System.Text.RegularExpressions.Regex.Match(aiTextParse ?? "", @"\[[\s\S]*\]");
        
        if(!cleanJson.Success)
        {
            return BadRequest("AI response did not contain valid JSON.");
        }
        
        var results = JsonSerializer.Deserialize<List<CareerMatch>>(cleanJson.Value, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        return Ok(results);
    }
}