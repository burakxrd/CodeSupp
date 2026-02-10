using AutoMapper;
using CodeSupp.Data;
using CodeSupp.Mappings;
using CodeSupp.Middlewares;
using CodeSupp.Services.Customers;
using CodeSupp.Services.Dashboard;
using CodeSupp.Services.Finance;
using CodeSupp.Services.Identity;
using CodeSupp.Services.Infrastructure;
using CodeSupp.Services.Integration;
using CodeSupp.Services.Products;
using CodeSupp.Services.Sales;
using CodeSupp.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// --- 1. ALTYAPI SERVÝSLERÝ ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// --- BUSINESS SERVICES ---
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISaleQueries, SaleQueries>();
builder.Services.AddScoped<ISaleCommands, SaleCommands>();
builder.Services.AddScoped<IIntegrationService, IntegrationService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();


// --- AUTOMAPPER ---
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
}, NullLoggerFactory.Instance);

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// --- VALIDATION ---
builder.Services.AddValidatorsFromAssemblyContaining<CreateSaleValidator>();

// --- 2. VERÝTABANI ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- 3. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

// --- 4. IDENTITY ---
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- 5. JWT ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Production'da SSL varsa true yapýlabilir

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidIssuer = jwtSettings["Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        ClockSkew = TimeSpan.Zero
    };

    // Sadece fonksiyonel kýsýmlarý býraktýk, debug loglarý temizlendi.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Token Header'da yoksa Cookie'ye bak (Bu mantýk önemli)
            if (context.Request.Cookies.ContainsKey("X-Access-Token"))
            {
                context.Token = context.Request.Cookies["X-Access-Token"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// --- 6. CONTROLLER ---
builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// --- 7. SWAGGER ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- PIPELINE ---
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CodeSupp.Data.ApplicationDbContext>();
    dbContext.Database.Migrate();
}
app.Run();