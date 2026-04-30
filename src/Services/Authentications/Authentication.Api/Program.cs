var builder = WebApplication.CreateBuilder(args);

// =========================
// 1) Configuration
// =========================
var env = builder.Environment.EnvironmentName;

var configuration = new ConfigurationBuilder()
    .AddJsonFile(APPSETTING_JSON, optional: false, reloadOnChange: true)
    .AddJsonFile(string.Format(APPSETTING_DEVELOPMENT_JSON, env), optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// =========================
// 2) Services
// =========================

// Observability
builder.Services.AddConfiguredLogging(configuration);
builder.Services.AddConfiguredOpenTelemetry(configuration);
builder.Services.AddConfiguredOptions(configuration);

// Security / Auth
// builder.Services.AddAuthServices();

// Caching / GCP
builder.Services.AddDistributedCache(configuration);
builder.Services.AddGcpSecretManagerInfrastructure(configuration);

// Core infra + app layers
builder.Services.AddCoreInfrastructure();
builder.Services.AddApiVersioningInfrastructure();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddApplication();

// Health checks
builder.Services.AddConfiguredHealthChecks();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(configurePolicy =>
    {
        configurePolicy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers / MVC
builder.Services.AddControllers(o =>
       {
           o.Filters.Add<BaseResponseFilter>();
           o.Filters.Add<LogContextFilter>();
           o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyTransformer()));
           o.ValueProviderFactories.Add(new KebabCaseRouteValueProviderFactory());
       })
       .AddJsonOptions(opts =>
       {
           opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
           opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
       })
       .ConfigureApiBehaviorOptions(o =>
       {
           o.InvalidModelStateResponseFactory = ValidationErrorResponseFactory.Create;
       });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddConfiguredSwagger();

// Kestrel
builder.WebHost.ConfigureKestrel((context, options) =>
{
    if (context.HostingEnvironment.IsDevelopment())
    {
        options.Limits.MinRequestBodyDataRate = null;
        options.Limits.MinResponseDataRate = null;
    }
    else
    {
        options.Limits.MinRequestBodyDataRate = new MinDataRate(
            bytesPerSecond: 100,
            gracePeriod: TimeSpan.FromSeconds(10));
    }
});

var app = builder.Build();

// =========================
// 3) Middleware pipeline
// =========================

// Configure the HTTP request pipeline.

// Swagger
app.UseSwaggerConfiguration();

app.UseHttpsRedirection();

// Routing & middlewares
app.UseRouting();
app.UseApplicationMiddlewares();
app.UseMiddleware<HttpErrorResponseMiddleware>();

// Security (CORS + Auth)
// CORS should be between UseRouting and UseAuthorization (recommendation)
app.UseCors();

// Endpoints
app.UseHealthChecks();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();