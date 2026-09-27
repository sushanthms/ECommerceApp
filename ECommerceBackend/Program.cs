using ECommerceBackend;
using ECommerceBackend.Data;
using ECommerceBackend.Logging;
using ECommerceBackend.Payments.Services;
using ECommerceBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;// allows Encoding

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
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing in configuration.");

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

            IssuerSigningKey = new SymmetricSecurityKey(// for checking the token we need key
                Encoding.UTF8.GetBytes(jwtKey)
            )
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Correlation-ID");
    });
});

// This takes everything we've configured and creates the actual ASP.NET Core application.
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // ASP.NET Core's DI container creates/gets an AppDbContext instance for this scope and stores that instance in context.
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // calls the SeedAdmin function, SeedAdmin is the method inside AdminSeeder class
    AdminSeeder.SeedAdmin(context, builder.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();// If an HTTP request should use HTTPS, it redirects it to HTTPS. 
app.UseStaticFiles();// ASP.NET Core can serve static files directly to the browser.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();
// the ASP.NET Core already knows which methods have what attributes([Authorize]. the app.mapController is to map the requets to that method to execute the method
app.MapControllers();// This connects incoming HTTP requests to the controllers. finds the controllers and maps their routes to the HTTP endpoints.

app.Run();