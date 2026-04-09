using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
[ApiController]
[Route("api/[controller]")]
public class CareerAPIController : ControllerBase 
{
    
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public CareerAPIController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _configuration = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("analyse")]
    public async Task<IActionResult> Analyse([FromBody] string cvText)
    {
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
                    content= $"Analyse the following CV text and provide potential job matches and suggest realistic career pivots. Return ONLY a JSON array with 'title' and 'desc' keys: \n\n{cvText}"
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
        
        var cleanJson = System.Text.RegularExpressions.Regex.Match(aiTextParse, @"\[[\s\S]*\]").Value;
        if(string.IsNullOrEmpty(cleanJson))
        {
            return BadRequest("AI response did not contain valid JSON.");
        }
        return Ok(cleanJson);
    }
    
    
    [HttpGet]
    public IActionResult Get() => Ok("API is working");
}