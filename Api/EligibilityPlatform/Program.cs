using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using EligibilityPlatform;
using EligibilityPlatform.Application.Middleware;
using EligibilityPlatform.Application.Repository;
using EligibilityPlatform.Application.Services;
using EligibilityPlatform.Application.Services.Inteface;
using EligibilityPlatform.Application.UnitOfWork;
using EligibilityPlatform.Domain.Models;
using EligibilityPlatform.Infrastructure.Context;
using EligibilityPlatform.Infrastructure.Middleware;
using EligibilityPlatform.Infrastructure.Repository;
using EligibilityPlatform.Infrastructure.UnitOfWork;
using EligibilityPlatform.Middleware;
using EligibilityPlatform.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Middleware;
using Serilog;
using AuthenticationService = EligibilityPlatform.Application.Services.AuthenticationService;
using IAuthenticationService = EligibilityPlatform.Application.Services.Inteface.IAuthenticationService;

Log.Information("Starting 3M Eligibility Platform API application.");

Serilog.Debugging.SelfLog.Enable(msg => Console.Error.WriteLine(msg));

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditInterceptor>();

builder.Services.AddDbContext<EligibilityDbContext>((sp, options) =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions =>
        {
            mySqlOptions.CommandTimeout(600); // 10 minutes
        });

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();

    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
});
 
builder.Services.AddControllers(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IValidatorService, ValidatorService>();
builder.Services.AddScoped<IApiDetailService, ApiDetailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IConditionService, ConditionService>();
builder.Services.AddScoped<ICountryService, CountryService>();
//builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IBulkImportService, BulkImportService>();
builder.Services.AddScoped<IParameterService, ParameterService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IDataTypeService, DataTypeService>();
builder.Services.AddScoped<IEcardService, EcardService>();
builder.Services.AddScoped<IEruleService, EruleService>();
builder.Services.AddScoped<IFactorService, FactorService>();
builder.Services.AddScoped<IHistoryEcService, HistoryEcService>();
builder.Services.AddScoped<IHistoryErService, HistoryErService>();
builder.Services.AddScoped<IHistoryParameterService, HistoryParameterService>();
builder.Services.AddScoped<IHistoryPcService, HistoryPcService>();
builder.Services.AddScoped<IListItemService, ListItemService>();
builder.Services.AddScoped<IManagedListService, ManagedListService>();
builder.Services.AddScoped<IMappingfunctionService, MappingFunctionService>();
builder.Services.AddScoped<INodeService, NodeService>();
builder.Services.AddScoped<INodeApiService, NodeApiService>();
builder.Services.AddScoped<IParamtersMapService, ParamtersMapService>();
builder.Services.AddScoped<IPcardService, PcardService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductParamservice, ProductParamService>();
//builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISecurityGroupService, SecurityGroupService>();
builder.Services.AddScoped<IUserGroupService, UserGroupService>();
builder.Services.AddScoped<IScreenService, ScreenService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IGroupRoleService, GroupRoleService>();
builder.Services.AddScoped<IUserStatusService, UserStatusService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IEligibilityApiService, EligibilityApiService>();
builder.Services.AddScoped<IMakerCheckerService, MakerCheckerService>();
builder.Services.AddScoped<ISettingService, SettingService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IAppSettingService, AppSettingService>();
builder.Services.AddScoped<IExceptionManagementService, ExceptionManagementService>();
builder.Services.AddScoped<IAmountEligibilityService, AmountEligibilityService>();
//builder.Services.AddScoped<IExceptionParameterService, ExceptionParameterService>();
//builder.Services.AddHostedService<ExceptionManagementBackgroundService>();
builder.Services.AddScoped<IApiParametersService, ApiParametersService>();
builder.Services.AddScoped<IApiResponsesService, ApiResponsesService>();
builder.Services.AddScoped<IApiParameterMapservice, ApiParameterMapservice>();
//builder.Services.AddScoped<IApiExcuteService, ApiExcuteService>();
//builder.Services.AddScoped<IApiIntegrationService, ApiIntegrationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IApiClientService, ApiClientService>();
builder.Services.AddScoped<IEligibleProductsService, EligibleProductsService>();
builder.Services.AddScoped<IProductCapService, ProductCapService>();
builder.Services.AddScoped<IEvaluationHistoryService, EvaluationHistoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEruleMasterService, EruleMasterService>();
builder.Services.AddScoped<IProductCapAmountService, ProductCapAmountService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<INodeApiRepository, NodeApiRepository>();
builder.Services.AddScoped<ILdapService, LdapService>();
//builder.Services.AddScoped<IDynamicApiService, DynamicApiService>();

// Also register other repositories if needed
builder.Services.AddScoped<INodeModelRepository, NodeRepository>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).
AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.Audience = builder.Configuration["Keycloak:Audience"];
    options.RequireHttpsMetadata = true;
    options.MetadataAddress = builder.Configuration["Keycloak:MetadataAddress"]!;
});
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "3MEligibility Platform API",
    });
});


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        //Description = "Please enter a valid JWT token",
        Description = "JWT Authorization header using the Bearer scheme.  Enter 'Bearer' [space] and then your token in the text input below.Example: \\\"Bearer 12345abcdef\\\"\"",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    //c.DocumentFilter<DynamicSwaggerDocumentFilter>();

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "x-api-key",
        Type = SecuritySchemeType.ApiKey,
        Description = "API Key needed to access endpoints"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
var rateLimitSection = builder.Configuration.GetSection("RateLimit");
int GetInt(string key, int defaultValue)
{
    return int.TryParse(rateLimitSection[key], out var value)
        ? value
        : defaultValue;
}
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {

        if (httpContext.Request.Path.StartsWithSegments("/api/log"))
        {
            return RateLimitPartition.GetNoLimiter(string.Empty); // no rate limit
        }
        // CRITICAL: Combine IP + UserID for tracking (prevents VPN/proxy bypass)
        var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userIdentifier = userId != null
            ? $"user-{userId}"
            : $"ip-{httpContext.Connection.RemoteIpAddress}";

        var path = httpContext.Request.Path;

        if (path.StartsWithSegments("/api/user/login") ||
            path.StartsWithSegments("/api/Authentication/login"))
        {

            int loginLimit = GetInt("Login_PermitLimit", 5);
            int loginWindow = GetInt("Login_WindowMinutes", 15);

            return RateLimitPartition.GetFixedWindowLimiter(userIdentifier, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginLimit,
                Window = TimeSpan.FromMinutes(loginWindow),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        }

        // 2. PASSWORD RESET ENDPOINTS - Prevent account takeover
        if (path.StartsWithSegments("/api/user/forgotpassword") ||
            path.StartsWithSegments("/api/user/resetpassword") ||
            path.StartsWithSegments("/api/user/requestresetpassword"))
        {
            int loginLimit = GetInt("PasswordReset_PermitLimit", 5);
            int loginWindow = GetInt("PasswordReset_WindowMinutes", 30);


            return RateLimitPartition.GetFixedWindowLimiter(userIdentifier, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = loginLimit,
                Window = TimeSpan.FromMinutes(loginWindow),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        }

        var method = httpContext.Request.Method;

        if (method == "GET")
        {
            int getLimit = GetInt("Get_PermitLimit", 60);
            int getWindow = GetInt("Get_WindowMinutes", 1);

            return RateLimitPartition.GetFixedWindowLimiter(userIdentifier, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = getLimit,            // Max 30 GET requests
                Window = TimeSpan.FromMinutes(getWindow),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        }
        if (method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH")
        {
            int writeLimit = GetInt("Write_PermitLimit", 30);
            int writeWindow = GetInt("Write_WindowMinutes", 1);
            return RateLimitPartition.GetFixedWindowLimiter(userIdentifier, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = writeLimit,
                Window = TimeSpan.FromMinutes(writeWindow),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
        }

        return RateLimitPartition.GetFixedWindowLimiter(userIdentifier, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {

        await Task.CompletedTask;
    };
});

var app = builder.Build();



// Use ExceptionHandlingMiddleware for centralized exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

}
app.UseRouting();


app.UseSwagger();
app.UseSwaggerUI();
app.UseSession();

//app.Use(async (context, next) =>
//{
//    var token = context.Session.GetString("Token");
//    if (!string.IsNullOrEmpty(token))
//    {
//        context.Request.Headers.Add("Authorization", "Bearer " + token);
//    }
//    await next();
//});

app.UseHttpsRedirection();
// set context oath 
//app.UsePathBase("/api");
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseMiddleware<UserSyncMiddleware>();

app.UseMiddleware<RoleBasedAuthorizationMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();


app.UseMiddleware<PermissionBasedAuthorizationMiddleware>();
app.MapControllers();
//app.MapDynamicApis();
try
{
    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

app.Run();
