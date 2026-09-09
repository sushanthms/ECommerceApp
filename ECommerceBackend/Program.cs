using ECommerceBackend;
using ECommerceBackend.Data;
using ECommerceBackend.Logging;
using ECommerceBackend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text; // allows Encoding

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers();

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
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!
                )
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
            .AllowAnyMethod();
    });
});

// This takes everything we've configured and creates the actual ASP.NET Core application.
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    AdminSeeder.SeedAdmin(context, builder.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();// If an HTTP request should use HTTPS, it redirects it to HTTPS. 
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();// This connects incoming HTTP requests to the  controllers. finds the controllers and maps their routes to HTTP endpoints.

app.Run();