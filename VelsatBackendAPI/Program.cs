using APIDatero.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VelsatBackendAPI.Data.Repositories;
using VelsatBackendAPI.Data.Services;
using VelsatBackendAPI.Hubs;
using MySqlConfiguration = VelsatBackendAPI.Data.MySqlConfiguration;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Configuration.AddJsonFile("appsettings.json");

var secretkey = builder.Configuration.GetSection("settings").GetSection("secretkey").Value;
var keyBytes = Encoding.UTF8.GetBytes(secretkey);

builder.Services.AddAuthentication(config =>
{
    config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mysqlConfiguration = new MySqlConfiguration(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    builder.Configuration.GetConnectionString("SecondConnection"),
    builder.Configuration.GetConnectionString("GtsConnection")
);

builder.Services.AddSingleton(mysqlConfiguration);
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Registrar los Repositories individuales
builder.Services.AddScoped<IDateroRepository, DateroRepository>();
builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<IHistoricosRepository, HistoricosRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IKilometrosRepository, KilometrosRepository>();
builder.Services.AddScoped<IDatosCargainicialService, DatosCargainicialService>();
builder.Services.AddScoped<IServidorRepository, ServidorRepository>();
builder.Services.AddScoped<IUrbanoAsignaService, UrbanoAsignaService>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//builder.Services.AddScoped<LoginController>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
        // Elimina AllowCredentials() y SetIsOriginAllowed()
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
});

builder.Services.AddSignalR(o =>
{
    o.EnableDetailedErrors = true;
});

//Descomentar para 107 - Envío de correos
//builder.Services.AddScoped<IDbConnection>(sp =>
//    new MySql.Data.MySqlClient.MySqlConnection(
//        builder.Configuration.GetConnectionString("DefaultConnection")
//    ));
//builder.Services.AddHostedService<AlertaCorreoService>();
//HASTA ACÁ

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//  app.UseSwagger();
//  app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Mapear todo de forma moderna
app.MapControllers();

app.MapHub<ActualizacionTiempoReal>("/dataHubDevice/{username}");

app.MapHub<ActualizacionVehiculoTiempoReal>("/dataHubVehicle/{username}/{placa}");

app.MapHub<UrbanoAsignaHub>("/urbanoHub");

app.Run();