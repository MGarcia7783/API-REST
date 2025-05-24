using App.Data;
using App.Repositorio.IRepositorio;
using App.Repositorio;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using App.PeliculasMappers;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using App.Modelos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using App.Validators;

var builder = WebApplication.CreateBuilder(args);

//Cargar valores de conexion
DotNetEnv.Env.Load();

//Leer variables de entorno
var host = Environment.GetEnvironmentVariable("DB_HOST");
var port = Environment.GetEnvironmentVariable("DB_PORT");
var database = Environment.GetEnvironmentVariable("DB_NAME");
var user = Environment.GetEnvironmentVariable("DB_USER");
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

// Construir la cadena
var connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";

/*
//Registrar conexion a base de datos
var connectionString = builder.Configuration.GetConnectionString("Connection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexi�n 'Connection' no est� configurada.");
}*/

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});




//Soporte para autenticaci�n con .NET Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
{
    //No quiero que Identity aplique reglas autom�ticas a las contrase�as.
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 0;
})
.AddPasswordValidator<ValidarPassword>()
.AddEntityFrameworkStores<ApplicationDbContext>();


//Registrar el validador de contrase�a
//Yo me encargo de validar las contrase�as con esta clase personalizada
builder.Services.AddTransient<IPasswordValidator<Usuario>, ValidarPassword>();


//Soporte para versionamiento
var apiVersioningBuilder = builder.Services.AddApiVersioning(opcion =>
{
    opcion.AssumeDefaultVersionWhenUnspecified = true;
    opcion.DefaultApiVersion = new ApiVersion(1, 0);
    opcion.ReportApiVersions = true;
});

apiVersioningBuilder.AddApiExplorer(
        opciones =>
        {
            opciones.GroupNameFormat = "'v'VVV";
            opciones.SubstituteApiVersionInUrl = true;
        }
);


//Registrar los Repositorios
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IPeliculaRepositorio, PeliculaRepositorio>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();



//Soporte para la clave secreta
var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");
if (string.IsNullOrWhiteSpace(key))
{
    throw new InvalidOperationException("La clave JWT (ApiSettings:Secreta) no est� configurada correctamente.");
}


// Aqui se configura la autenticaci�n
builder.Services.AddAuthentication
    (
        x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }
    ).AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role
        };
    });


//Registrar el AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));


//Registrar controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "Autenticaci�n JWT usando el esquema Bearer. \r\n\r\n " +
        "Ingresa la palabra 'Bearer' seguido de un [espacio] y despu�s su token en el campo de abajo.\r\n\r\n" +
        "Ejemplo: \"Bearer tkljk125jhhk\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",
        Title = "Peliculas Api V1",
        Description = "Api de Peliculas Versi�n 1",
        TermsOfService = new Uri("https://render2web.com/promociones"),
        Contact = new OpenApiContact
        {
            Name = "render2web",
            Url = new Uri("https://render2web.com/promociones")
        },
        License = new OpenApiLicense
        {
            Name = "Licencia Personal",
            Url = new Uri("https://render2web.com/promociones")
        }
    }
    );
    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2.0",
        Title = "Peliculas Api V2",
        Description = "Api de Peliculas Versi�n 2",
        TermsOfService = new Uri("https://render2web.com/promociones"),
        Contact = new OpenApiContact
        {
            Name = "render2web",
            Url = new Uri("https://render2web.com/promociones")
        },
        License = new OpenApiLicense
        {
            Name = "Licencia Personal",
            Url = new Uri("https://render2web.com/promociones")
        }
    }
    );
}
 );


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "APIPel�culasV1");
        opciones.SwaggerEndpoint("/swagger/v2/swagger.json", "APIPel�culasV2");
    });
}



// Soporte para archivos est�ticos como im�genes
app.UseStaticFiles();


// Usar middleware para HTTPS Redirection y autorizaci�n
app.UseHttpsRedirection();


// Soporte para autenticaci�n
app.UseAuthentication();
app.UseAuthorization();


// Mapear controladores
app.MapControllers();


// Ejecutar la aplicaci�n
app.Run();
