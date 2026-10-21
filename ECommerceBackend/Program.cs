using ECommerceBackend;
using ECommerceBackend.Data;
using ECommerceBackend.Hubs;
using ECommerceBackend.Logging;
using ECommerceBackend.Services;
using ECommerceBackend.Services.Caching;
using ECommerceBackend.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;// allows Encoding
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// puts the SQL Server configuration and connection string into the options object.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var loggingProvider = builder.Configuration["LoggingSettings:Provider"];

if (loggingProvider == "File")
{
    builder.Services.AddScoped<IApplicationLogger, FileLogger>();// means when somewhere logger is used, it understands it is referred to FileLogger
}
else if (loggingProvider == "Database")
{
    builder.Services.AddScoped<IApplicationLogger, DatabaseLogger>();// means when somewhere logger is used, it understands it is referred to DatabaseLogger
}
else
{
    throw new Exception("Invalid logging provider. Use File or Database.");
}

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BannerService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<PaymentWebhookService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

var jwtKey = builder.Configuration["Jwt:Key"]?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");

if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters long.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,// checking
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,// checks the signature, signature is for the jwt.

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ClockSkew = TimeSpan.FromSeconds(30),

            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // frontend first sends http request to establish connection between the backend
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 6,
                QueueLimit = 0
            }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:8080"
                )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("X-Correlation-ID");
    });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = "ecommerce:";
});

builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// This takes everything we've configured and creates the actual ASP.NET Core application.
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    AdminSeeder.SeedAdmin(context, builder.Configuration);// gets admin data from the database or creates admin data in the database
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();// ASP.NET Core can serve static files directly to the browser.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseCors("ReactPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
// the ASP.NET Core already knows which methods have what attributes([Authorize]. the app.mapController is to map the requets to that method to execute the method
app.MapControllers();// This connects incoming HTTP requests to the controllers. finds the controllers and maps their routes to the HTTP endpoints.
app.MapHub<NotificationHub>("/notificationHub");// means any request(to establish connection between the frontend and the backend) to the URL /notificationHub is handled by the NotificationHub class

app.Run();