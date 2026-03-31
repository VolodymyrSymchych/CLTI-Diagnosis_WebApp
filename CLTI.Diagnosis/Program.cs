// /app/CLTI.Diagnosis/Program.cs
using CLTI.Diagnosis.Components;
using CLTI.Diagnosis.Client.Infrastructure.Auth;
using CLTI.Diagnosis.Client.Infrastructure.Http;
using CLTI.Diagnosis.Client.Infrastructure.State;
using CLTI.Diagnosis.Client.Features.Diagnosis.Services;
using CLTI.Diagnosis.Infrastructure.Data;
using CLTI.Diagnosis.Services;
using CLTI.Diagnosis.Middleware;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using CLTI.Diagnosis.Data;
using Serilog;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// API controllers
builder.Services.AddControllers();

// DB context
var databaseUrl =
    builder.Configuration["DATABASE_URL"]
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

var rawConnectionString =
    databaseUrl
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found (or DATABASE_URL not set).");

static bool LooksLikePostgres(string cs) =>
    cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
    || cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
    || cs.Contains("Host=", StringComparison.OrdinalIgnoreCase)
    || cs.Contains("Username=", StringComparison.OrdinalIgnoreCase);

var usePostgres = !string.IsNullOrWhiteSpace(databaseUrl) || LooksLikePostgres(rawConnectionString);
var connectionString = usePostgres ? NormalizePostgresConnectionString(rawConnectionString) : rawConnectionString;

static string NormalizePostgresConnectionString(string cs)
{
    if (cs.StartsWith("Host=", StringComparison.OrdinalIgnoreCase) ||
        cs.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))
    {
        return cs;
    }

    if (!cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return cs;
    }

    var uri = new Uri(cs);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.Trim('/');

    var port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port;

    var parts = new List<string>
    {
        $"Host={uri.Host}",
        $"Port={port}",
        $"Database={database}",
        $"Username={username}",
        $"Password={password}"
    };

    if (!string.IsNullOrWhiteSpace(uri.Query))
    {
        var queryPairs = uri.Query.TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var pair in queryPairs)
        {
            var kv = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(kv[0]);
            var value = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add($"SSL Mode={value}");
                continue;
            }

            if (key.Equals("channel_binding", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add($"Channel Binding={value}");
                continue;
            }

            var normalizedKey = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                key.Replace("_", " ").ToLowerInvariant());
            parts.Add($"{normalizedKey}={value}");
        }
    }

    return string.Join(";", parts);
}

static async Task EnsurePostgresCaseSchemaAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("PostgresSchemaBootstrap");

    var commands = new[]
    {
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "PatientFullName" character varying(200) NOT NULL DEFAULT 'Без імені';""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "CaseStatus" character varying(20) NOT NULL DEFAULT 'Open';""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "LastVisitedStep" character varying(256);""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "LastClosedStep" character varying(256);""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsWCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsICompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsfICompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsWiFIResultsCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsCRABCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "Is2YLECompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsSurgicalRiskCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsGLASSCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsGLASSFemoroPoplitealCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsGLASSInfrapoplitealCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsGLASSFinalCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsSubmalleolarDiseaseCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsRevascularizationAssessmentCompleted" boolean NOT NULL DEFAULT FALSE;""",
        """ALTER TABLE "u_clti" ADD COLUMN IF NOT EXISTS "IsRevascularizationMethodCompleted" boolean NOT NULL DEFAULT FALSE;"""
    };

    foreach (var sql in commands)
    {
        await db.Database.ExecuteSqlRawAsync(sql);
    }

    logger.LogInformation("Postgres schema bootstrap for u_clti completed.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (usePostgres)
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// ✅ DATA PROTECTION (required for session encryption)
// Persist keys so session cookies survive app restarts
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "Keys");
Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("CLTI.Diagnosis")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

// ✅ SESSION STORAGE (Server-side - works even if cookies blocked)
if (usePostgres)
{
    // Postgres: keep session storage simple and avoid SQL Server SessionCache initializer.
    // If you need multi-instance persistence later, swap this for Redis.
    builder.Services.AddDistributedMemoryCache();
}
else
{
    // SQL Server distributed cache for session storage
    builder.Services.AddDistributedSqlServerCache(options =>
    {
        options.ConnectionString = connectionString;
        options.SchemaName = "dbo";
        options.TableName = "SessionCache";
        options.DefaultSlidingExpiration = TimeSpan.FromMinutes(20);
    });

    // Initialize SessionCache table if it doesn't exist
    builder.Services.AddSingleton<CLTI.Diagnosis.Infrastructure.Services.SessionCacheInitializer>();
}

// ✅ SESSION CONFIGURATION (works without cookies via headers)
builder.Services.AddSession(options =>
{
    // ✅ Longer timeout - sessions persist across server restarts (using SQL Server cache)
    options.IdleTimeout = TimeSpan.FromHours(24); // 24 hours - user stays logged in after restart
    
    options.Cookie.Name = ".AspNetCore.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    // ✅ Session cookie expires after 24 hours (matches idle timeout)
    // This ensures session survives server restarts
    // Session works via cookie OR custom header if cookies blocked
});

// JWT CONFIGURATION - PURE JWT, NO COOKIES
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-min-256-bits-long-for-security-purposes-12345";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CLTI.Diagnosis";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CLTI.Diagnosis.Client";

builder.Services.AddAuthentication(options =>
{
    // ✅ ОСНОВНОЮ СХЕМОЮ РОБИМО COOKIES ДЛЯ BLAZOR SERVER
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignOutScheme = IdentityConstants.ApplicationScheme;
})
// ✅ COOKIE AUTHENTICATION ДЛЯ BLAZOR SERVER (ОСНОВНЕ)
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;

    options.Cookie.Name = ".AspNetCore.Identity.Application";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // ✅ НАЛАШТУВАННЯ ДЛЯ BLAZOR SERVER
    options.Events.OnRedirectToLogin = context =>
    {
        // Для API запитів повертаємо 401 замість редиректу
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Headers.Accept.ToString().Contains("application/json") ||
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }

        // Для звичайних запитів робимо редирект
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") ||
            context.Request.Headers.Accept.ToString().Contains("application/json"))
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
})
// ✅ JWT AUTHENTICATION ДЛЯ API ENDPOINTS (ДОДАТКОВЕ)
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // ✅ Read token from Authorization header (added by SessionTokenMiddleware)
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogDebug("JWT token extracted from Authorization header | Path: {Path}", context.Request.Path);
                return Task.CompletedTask;
            }

            // Fallback: read from query string for Blazor SignalR
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/_blazor"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Authentication failed: {Error}", context.Exception.Message);
            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("JWT Challenge triggered for path: {Path}", context.Request.Path);

            // ✅ НЕ РОБИМО АВТОМАТИЧНИЙ CHALLENGE ДЛЯ JWT
            // Це дозволить основній схемі (cookies) обробити автентифікацію
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// ✅ AUTHORIZATION З ПОЛІТИКАМИ ДЛЯ РІЗНИХ СХЕМ
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    // Політика для Blazor сторінок що використовують cookies
    options.AddPolicy("WebPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme);
        policy.RequireAuthenticatedUser();
    });

    // Комбінована політика (підтримує обидві схеми)
    options.AddPolicy("HybridPolicy", policy =>
    {
        policy.AddAuthenticationSchemes(
            IdentityConstants.ApplicationScheme,
            JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
});

// ✅ IDENTITY ДЛЯ BLAZOR SERVER
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

// Application services
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddSingleton<StateService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<CLTI.Diagnosis.Services.CltiCaseService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<CLTI.Diagnosis.Infrastructure.Services.IPasswordHasherService, 
    CLTI.Diagnosis.Infrastructure.Services.PasswordHasherService>();
builder.Services.AddScoped<CLTI.Diagnosis.Infrastructure.Services.ISessionStorageService,
    CLTI.Diagnosis.Infrastructure.Services.SessionStorageService>();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// HttpClient for OpenAI
builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "CLTI-Diagnosis/1.0");
    client.Timeout = TimeSpan.FromSeconds(60);
});

// ✅ HTTP CLIENT ДЛЯ ВНУТРІШНІХ API ЗАПИТІВ
builder.Services.AddHttpClient("InternalApi", (sp, client) =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    string baseUrl;
    if (environment.IsDevelopment())
    {
        baseUrl = "https://localhost:7124";
    }
    else
    {
        // Read base URL from configuration
        baseUrl = configuration["InternalApi:BaseUrl"] ?? "https://antsdemo08.demo.dragon-cloud.org";
    }

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", "CLTI-Diagnosis-Client");

}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    UseCookies = true,
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

// ✅ DEFAULT HTTP CLIENT для Blazor Server
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    var client = factory.CreateClient("InternalApi");

    // Для Blazor Server передаємо cookies з поточного HTTP контексту
    if (httpContext != null)
    {
        var cookieHeader = httpContext.Request.Headers.Cookie.ToString();
        if (!string.IsNullOrEmpty(cookieHeader))
        {
            client.DefaultRequestHeaders.Remove("Cookie");
            client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
        }

        // Передаємо інформацію про користувача
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                client.DefaultRequestHeaders.Remove("X-User-Id");
                client.DefaultRequestHeaders.Add("X-User-Id", userId);
            }
        }
    }

    return client;
});

// ✅ КЛІЄНТСЬКІ СЕРВІСИ
builder.Services.AddScoped<CltiApiClient>();
builder.Services.AddScoped<CLTI.Diagnosis.Client.Features.Diagnosis.Services.CltiCaseService>();
builder.Services.AddScoped<IUserClientService, UserClientService>();
builder.Services.AddScoped<IClientApiKeyService, ClientApiKeyService>();
builder.Services.AddScoped<AiChatClient>();
builder.Services.AddScoped<AuthApiService>();

// Logging is now handled by Serilog configuration in appsettings

// CORS
// ✅ IMPORTANT: AllowCredentials() cannot be used with AllowAnyOrigin()
// For cookie support, we need to specify specific origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        // ✅ Allow specific origins for development (supports both HTTP and HTTPS)
        if (builder.Environment.IsDevelopment())
        {
            policy.WithOrigins(
                "https://localhost:7124",
                "http://localhost:5276",
                "https://localhost:5276",
                "http://localhost:7124"
            )
            .AllowCredentials() // ✅ Required for cookies to work with CORS
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
        else
        {
            // Production: use configuration or specific origins
            var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() 
                ?? new[] { builder.Configuration["BaseUrl"] ?? "https://localhost:7124" };
            
            policy.WithOrigins(allowedOrigins)
            .AllowCredentials() // ✅ Required for cookies to work with CORS
            .AllowAnyHeader()
            .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// MIDDLEWARE PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();

if (!usePostgres)
{
    // Initialize SessionCache table before using session (SQL Server only)
    var sessionCacheInitializer = app.Services.GetRequiredService<CLTI.Diagnosis.Infrastructure.Services.SessionCacheInitializer>();
    sessionCacheInitializer.EnsureCreated();
}
else
{
    await EnsurePostgresCaseSchemaAsync(app.Services);
}

app.UseSession(); // ✅ Enable session middleware (must be before UseAuthentication)
app.UseMiddleware<CLTI.Diagnosis.Web.Middleware.SessionTokenMiddleware>(); // ✅ Auto-add session token to API requests
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(CLTI.Diagnosis.Client._Imports).Assembly);

// ✅ CONTROLLERS З МОЖЛИВІСТЮ ВИБОРУ ПОЛІТИКИ
app.MapControllers();

// ✅ LOGOUT ENDPOINT
app.MapPost("/Account/Logout", async (HttpContext context) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogDebug("User logout requested");

    await context.SignOutAsync(IdentityConstants.ApplicationScheme);

    // Очищаємо cookies
    context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
    context.Response.Cookies.Delete(".AspNetCore.Antiforgery.mYlosc6T-lA");

    logger.LogDebug("User logged out successfully");
    return Results.Redirect("/");
});

// ✅ HEALTH CHECK ENDPOINT
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName
    });
});

// Fallback route
app.MapFallback(async context =>
{
    var path = context.Request.Path.Value ?? "/";

    if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/Photo", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase) ||
        path.Contains('.', StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Not Found");
        return;
    }

    context.Response.Redirect($"/Error?path={Uri.EscapeDataString(path)}&type=404");
});

app.Run();
