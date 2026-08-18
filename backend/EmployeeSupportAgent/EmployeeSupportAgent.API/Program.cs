using System.IdentityModel.Tokens.Jwt;
using System.Text;
using EmployeeSupportAgent.API.Data;
using EmployeeSupportAgent.API.Infrastructure;
using EmployeeSupportAgent.API.Plugins;
using EmployeeSupportAgent.API.Repositories;
using EmployeeSupportAgent.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.SemanticKernel;

// Disable the legacy claim-type mapping so short claim names ("role", "name") are preserved.
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

var builder = WebApplication.CreateBuilder(args);

// MVC + Problem Details + global exception filter for RFC 7807 responses
builder.Services.AddProblemDetails();
builder.Services.AddControllers(o => o.Filters.Add<GlobalExceptionFilter>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Support Agent API",
        Version = "v1"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter JWT Token"
    });
});

builder.Services.AddCors(o => o.AddPolicy("ReactPolicy", p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
     .WithOrigins("http://localhost:5173", "http://localhost:3000")));

// ----- Database (provider-agnostic) -----
// Default provider is SQLite; override via `DatabaseProvider` in appsettings.
// Supported providers: Sqlite | SqlServer | MySql | Postgres.
// (MySql/Postgres providers currently track EF 9 on nuget.org; until EF 10 builds ship,
//  attempting to start with those providers throws a clear error.)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=ai-employee.db";
var provider = (builder.Configuration["DatabaseProvider"] ?? "Sqlite").Trim();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    switch (provider.ToLowerInvariant())
    {
        case "sqlite":
            opt.UseSqlite(connectionString);
            break;
        case "sqlserver":
            opt.UseSqlServer(connectionString);
            break;
        case "mysql":
            opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            break;
        case "postgres":
        case "postgresql":
            opt.UseNpgsql(connectionString);
            break;
        default:
            throw new InvalidOperationException(
                $"Unknown DatabaseProvider '{provider}'. Supported: Sqlite, SqlServer, MySql, Postgres.");
    }
});

// ----- Repositories -----
builder.Services.AddScoped(typeof(IRepository<>), typeof(EFRepository<>));
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<IITTicketRepository, ITTicketRepository>();
builder.Services.AddScoped<IPayslipRepository, PayslipRepository>();
builder.Services.AddScoped<IReimbursementRepository, ReimbursementRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

// ----- Services -----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<LeaveService>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<PayslipService>();
builder.Services.AddScoped<ReimbursementService>();
builder.Services.AddSingleton<IntentRouter>();
builder.Services.AddScoped<AgentService>();

// ----- Plugins -----
builder.Services.AddScoped<EmployeePlugin>();
builder.Services.AddScoped<LeavePlugin>();
builder.Services.AddScoped<TicketPlugin>();
builder.Services.AddScoped<PayslipPlugin>();
builder.Services.AddScoped<ReimbursementPlugin>();

// ----- Semantic Kernel -----
var kernelBuilder = builder.Services.AddKernel();

var useOpenAi = builder.Configuration.GetValue<bool>("Agent:UseOpenAI");
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
var openAiModel = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini";

if (useOpenAi && !string.IsNullOrWhiteSpace(openAiKey))
{
    kernelBuilder.AddOpenAIChatCompletion(modelId: openAiModel, apiKey: openAiKey);
}

// ----- JWT -----
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed.");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
