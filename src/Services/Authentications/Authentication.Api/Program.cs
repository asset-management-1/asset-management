var builder = WebApplication.CreateBuilder(args);

// Preserve the host configuration pipeline, including Development User Secrets, before applying
// the optional production Secret Manager overlay.
IConfiguration configuration = await builder.Configuration.ApplySecretsAsync();

// Observability
builder.Services.AddConfiguredLogging(configuration);
builder.Services.AddConfiguredOpenTelemetry(configuration);
builder.Services.AddConfiguredOptions(configuration);

// Security / Auth
builder.Services.AddAuthServices();

// Caching / external infrastructure
builder.Services.AddDistributedCache(configuration);
builder.Services.AddGcpSecretManagerInfrastructure(configuration);

// Core infra + app layers
builder.Services.AddCoreInfrastructure();
builder.Services.AddApiVersioningInfrastructure();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddApplication();
builder.Services.AddMediatorServices();

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
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddConfiguredSwagger();
    builder.Services.AddSwaggerGen(options =>
    {
        // Authentication token endpoints need device-header examples in Swagger Try-it-out.
        options.OperationFilter<ClientDeviceInfoSwaggerOperationFilter>();
    });
}

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

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerConfiguration();
}
else
{
    app.UseHsts();
}

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
