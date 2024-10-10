# C# Developer Test

Welcome! This test is designed to assess your familiarity with setting up a .NET 8 environment, using VS Code and the command line, and developing a simple ASP.NET Core API. You must be able to solve any issues arise.

Do not use AI assistance to resolve this test.

## Task Overview

You are required to create an ASP.NET Core API project using .NET 8 and VS Code. The project will expose two API endpoints that retrieve data from an external service, store it in a SQLite database using `Entity Framework Core`, and cache the responses.

### Requirements

1. **Setup .NET 8:**
   - Install the .NET 8 SDK from [Microsoft's official website](https://dotnet.microsoft.com/).
   - Download and install VS Code on your machine.
   - Ensure your environment is properly configured to use .NET 8.

2. **Create a New ASP.NET Core API Project:**
   - Use the command line to create a new ASP.NET Core API project without any authentication.
   - Use the built-in Microsoft Extension Logger for logging purposes.
   - Configure the project to use a SQLite database with `Entity Framework Core`.

3. **Model Creation:**
   - Create a model class in your project based on the [House structure](https://github.com/joakimskoog/AnApiOfIceAndFire/wiki/Houses) provided in the An API of Ice and Fire documentation.

4. **Generate API Controller:**
   - Use scaffolding via the command line to create an API controller named `HousesApiController`.
   - The controller should provide two endpoints:
     - A `GET` endpoint `/houses` to fetch all houses.
     - A `GET` endpoint `/houses/{id}` to fetch details of a specific house by its ID.
   - The controller should:
     - Fetch data from the external API: 
       - All houses: `https://www.anapioficeandfire.com/api/houses`.
       - A specific house by ID: `https://www.anapioficeandfire.com/api/houses/{id}`.
     - Store the data in the SQLite database.
     - Use in-memory caching to store each response for 1 minute or just cache the first response for subsequent requests.
     - Log when a cache hit occurs.

5. **Apply Migrations:**
   - Use Entity Framework Core to add and apply migrations to create the necessary database schema.

6. **Enable CORS:**
   - Allow cross-origin requests from any domain by enabling CORS in your ASP.NET Core project.

7. **Testing:**
   - To test your API, you will use a provided `index.html` file as the frontend, which will query your backend API.
   - The provided `index.html` file will only function correctly if your backend API is set up as required.

### Step-by-Step Instructions

#### 1. **Setup Environment**

Install the .NET 8 SDK and confirm your environment setup with:

```bash
dotnet --version
```

Download and install VS Code and ensure the necessary C# extensions are installed.

#### 2. **Create ASP.NET Core API Project**

Open a terminal and navigate to your preferred project directory. Create a new API project with:

```bash
dotnet new webapi -n HousesApi
cd HousesApi
```

Open the project in VS Code:

```bash
code .
```

#### 3. **Configure the Project**

Install the necessary packages for `Entity Framework Core` and `SQLite` support:

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Create a model class `House` based on the provided structure. Example:

```csharp
// Models/House.cs
public class House
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Region { get; set; }
    public string CoatOfArms { get; set; }
    public string Words { get; set; }
    // Add other relevant properties...
}
```

Create a `DbContext` class to manage the SQLite database:

```csharp
// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<House> Houses { get; set; }
}
```

Update `Program.cs` to configure the SQLite database and enable CORS:

```csharp
// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=houses.db"));
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();
app.MapControllers();
```

#### 4. **Generate API Controller**

Use the command line to scaffold the API controller using the `aspnet-codegenerator` tool:

```bash
dotnet tool install --global dotnet-aspnet-codegenerator
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet aspnet-codegenerator controller -m House -dc ApplicationDbContext -outDir Controllers --databaseProvider sqlite -api -name HousesApiController
```

This command generates the `HousesApiController` in the `Controllers` folder using the `House` model and `ApplicationDbContext` with SQLite as the database provider.

Update the `HousesApiController` to implement data fetching, storing in the SQLite database, and in-memory caching:

```csharp
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
        if (_cache.TryGetValue(cacheKey, out var house))
        {
            _logger.LogInformation($"Cache hit for house data with ID {id}.");
            return Ok(house);
        }

        _logger.LogInformation($"Fetching house data with ID {id} from external API.");
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
```

#### 5. **Apply Migrations**

To create the database schema, add an initial migration:

```bash
dotnet ef migrations add InitialCreate
```

Then, update the database with the new migration:

```bash
dotnet ef database update
```

#### 6. **Run and Test the API**

Run the API using:

```bash
dotnet run
```

Open the provided `index.html` file in a browser to ensure it can communicate correctly with your backend.

### Additional Notes

- **Database Strategy:** Use `Entity Framework Core` with a SQLite database to store data.
- **Caching Strategy:** Use in-memory caching (`IMemoryCache`) to store the response. Ensure that each response is cached for 1 minute or until the next request.
- **CORS Configuration:** Ensure CORS is enabled to allow requests from any origin.
- **Logging:** Make sure that the logger outputs a message when

 the cache is hit, indicating that the data is served from the cache.
- **Error Handling:** Ensure proper error handling when fetching data from the external API.

## Submission

- Zip your project directory and submit it along with a brief README describing how to run your API.
- Ensure all dependencies and requirements are clearly listed.

## Good Luck!

We look forward to seeing your solution!
