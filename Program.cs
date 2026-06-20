using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Db;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using La_Divine.Src.Services.Interfaces;
using La_Divine.Src.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ContextDb>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICloudinaryServices, CloudinaryServices>();

builder.Services.AddScoped<IAuthServices, AuthServices>();

builder.Services.AddScoped<IFunctionServices, FunctionServices>();

builder.Services.AddScoped<ISendGridEmailServices, SendGridEmailServices>();

builder.Services.AddScoped<IReservationServices, ReservationServices>();
// Servicio que genera la entrada en PDF de una reserva (FUN-005)
builder.Services.AddScoped<IPdfServices, PdfServices>();

// Servicio en segundo plano que libera asientos de reservas no pagadas tras 10 minutos
builder.Services.AddHostedService<ReservationCleanupService>();

// Registro del servicio de pagos con HttpClient tipado: reutiliza conexiones TCP y aplica timeout (REQ NF07)
builder.Services.AddHttpClient<IPaymentService, PaymentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentApi:BaseUrl"]!);
    client.Timeout     = TimeSpan.FromSeconds(10);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173",
                            "https://divinecommit-frontend.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Licencia community de QuestPDF (requerida para generar PDFs - FUN-005)
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await Seeder.Initialize(app.Services);

app.Run();
