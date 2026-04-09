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
    public async Task<IActionResult> Analyse([FromBody]  String cvText)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var url = _configuration["https://googleapis.com{apiKey}"];
        
        var requestBody = new
        {
            contents = new[] {
                new { parts = new[] { new { text = $"Analyze this CV and suggest 4 career pivots in JSON format with 'title' and 'desc' keys: {cvText}" } } }
            }
        };

        var client =  _httpClientFactory.CreateClient();
        var response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));
        if(!response.IsSuccessStatusCode) return StatusCode((int)response.StatusCode);
        
        var JsonResponse = await response.Content.ReadAsStringAsync();
        return Ok(JsonResponse);
    }
    
    
    
    [HttpGet]
    public IActionResult Get() => Ok("API is working");
}