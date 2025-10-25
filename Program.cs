using Microsoft.EntityFrameworkCore;
using aspnetcoreapi.Data;
using aspnetcoreapi.Services;
using aspnetcoreapi.Repositories.Interfaces;
using aspnetcoreapi.Repositories.Implementations;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using aspnetcoreapi.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "aspnetcoreapi", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// AuthN/AuthZ
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
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
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" 
        };
    });
builder.Services.AddAuthorization();


// Add services to the container.
builder.Services.AddControllers();

// Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);

// Register services
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Functionality services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();


// midldleware validasi modelstatate
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .Select(x => x.Value!.Errors[0].ErrorMessage)
            .FirstOrDefault() ?? "Request validation failed.";

        var error = new ApiResponse
        {
            Title = "Bad Request",
            Status = 400,
            Message = message
        };
        return new BadRequestObjectResult(error);
    };
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5112") 
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); 
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Swagger middleware
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();
