// Controllers/HousesApiController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class HousesApiController : ControllerBase
{
    private readonly ILogger<HousesApiController> _logger;
    private readonly IMemoryCache _cache;
    private readonly ApplicationDbContext _context;
    private static readonly HttpClient _httpClient = new HttpClient();

    public HousesApiController(ILogger<HousesApiController> logger, IMemoryCache cache, ApplicationDbContext context)
    {
        _logger = logger;
        _cache = cache;
        _context = context;
    }

    // GET: api/houses
    [HttpGet]
    public async Task<IActionResult> GetAllHouses()
    {
        string cacheKey = "all_houses";
        if (_cache.TryGetValue(cacheKey, out var houses))
        {
            _logger.LogInformation("Cache hit for all houses data.");
            return Ok(houses);
        }

        _logger.LogInformation("Fetching all houses data from external API.");
        var response = await _httpClient.GetStringAsync("https://www.anapioficeandfire.com/api/houses");

        // Deserialize and store in SQLite database
        var houseList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<House>>(response);
        _context.Houses.AddRange(houseList);
        await _context.SaveChangesAsync();

        // Cache the response in memory for 1 minute
        _cache.Set(cacheKey, houseList, TimeSpan.FromMinutes(1));

        return Ok(houseList);
    }

    // GET: api/houses/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHouseById(int id)
    {
        string cacheKey = $"house_{id}";
        if (_cache.TryGetValue(cacheKey, out var List))
        {
            _logger.LogInformation($"Cache hit for List data with ID {id}.");
            return Ok(List);
        }

        _logger.LogInformation($"Fetching List data with ID {id} from external API.");
        var response = await _httpClient.GetStringAsync($"https://www.anapioficeandfire.com/api/houses/{id}");

        // Deserialize and store in SQLite database
        var houseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<House>(response);
        _context.Houses.Add(houseDetails);
        await _context.SaveChangesAsync();

        // Cache the response in memory for 1 minute
        _cache.Set(cacheKey, houseDetails, TimeSpan.FromMinutes(1));

        return Ok(houseDetails);
    }
}