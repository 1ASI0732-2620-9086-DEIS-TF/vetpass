using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using VetPass.API.IAM.Application.Internal.CommandServices;
using VetPass.API.IAM.Application.Internal.QueryServices;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.IAM.Domain.Services;
using VetPass.API.IAM.Infrastructure.Identity;
using VetPass.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using VetPass.API.MedicalRecords.Application.Internal.CommandServices;
using VetPass.API.MedicalRecords.Application.Internal.QueryServices;
using VetPass.API.MedicalRecords.Domain.Repositories;
using VetPass.API.MedicalRecords.Infrastructure.Persistence.EFC.Repositories;
using VetPass.API.Patients.Application.Internal.CommandServices;
using VetPass.API.Patients.Application.Internal.QueryServices;
using VetPass.API.Patients.Domain.Repositories;
using VetPass.API.Patients.Infrastructure.Persistence.EFC.Repositories;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Seeding;
using VetPass.API.Shared.Interfaces.ASP.Middleware;
using VetPass.API.Shared.Interfaces.ASP.Security;
using VetPass.API.Vaccination.Application.Internal.CommandServices;
using VetPass.API.Vaccination.Application.Internal.QueryServices;
using VetPass.API.Vaccination.Domain.Repositories;
using VetPass.API.Vaccination.Domain.Services;
using VetPass.API.Vaccination.Infrastructure.Persistence.EFC.Repositories;

var builder = WebApplication.CreateBuilder(args);

// En un contenedor (Vercel) la plataforma indica en PORT dónde debe escuchar
// la API. En desarrollo no existe y manda la configuración habitual.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------------------------------------------------------------- persistencia
builder.Services.AddDbContext<VetPassDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("VetPassDb")
                           ?? throw new InvalidOperationException(
                               "Falta la cadena de conexión 'VetPassDb'. Configúrala con dotnet user-secrets.");

    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", VetPassDbContext.Schema));

    if (builder.Environment.IsDevelopment())
        options.EnableDetailedErrors().EnableSensitiveDataLogging();
});

// ------------------------------------------------------- proveedor de identidad
builder.Services.Configure<SupabaseAuthOptions>(
    builder.Configuration.GetSection(SupabaseAuthOptions.SectionName));

var supabase = builder.Configuration.GetSection(SupabaseAuthOptions.SectionName).Get<SupabaseAuthOptions>()
               ?? new SupabaseAuthOptions();

builder.Services.AddHttpClient<IIdentityProvider, SupabaseAuthGateway>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = supabase.AuthUrl,
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = ClaimTypes.Role
        };

        // Los proyectos nuevos de Supabase firman con claves asimétricas y
        // publican su JWKS; los anteriores usan un secreto compartido. Se
        // admiten ambos para no atar el despliegue a la antigüedad del proyecto.
        var legacySecret = builder.Configuration["Supabase:JwtSecret"];
        if (!string.IsNullOrWhiteSpace(legacySecret))
        {
            options.TokenValidationParameters.IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(legacySecret));
        }
        else if (!string.IsNullOrWhiteSpace(supabase.Url))
        {
            options.Authority = supabase.AuthUrl;
            options.MetadataAddress = $"{supabase.AuthUrl}/.well-known/openid-configuration";
            options.RequireHttpsMetadata = true;
        }
        else if (!builder.Environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Falta la configuración de Supabase: sin ella no hay forma de validar los tokens.");
        }
    });

builder.Services.AddSingleton<IClaimsTransformation, SupabaseClaimsTransformation>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.ClinicStaff, policy =>
        policy.RequireClaim(CurrentUser.RoleClaim, AuthorizationPolicies.ClinicStaff))
    .AddPolicy(AuthorizationPolicies.PetOwner, policy =>
        policy.RequireClaim(CurrentUser.RoleClaim, AuthorizationPolicies.PetOwner));

// ------------------------------------------------------------ servicios propios
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IClinicClock, ClinicClock>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IClinicRepository, ClinicRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IVaccinationCardRepository, VaccinationCardRepository>();
builder.Services.AddScoped<IVaccineCatalogRepository, VaccineCatalogRepository>();
builder.Services.AddScoped<IVaccinationScheduleProvider, VaccinationScheduleProvider>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();

builder.Services.AddScoped<AuthenticationCommandService>();
builder.Services.AddScoped<UserProfileQueryService>();
builder.Services.AddScoped<PatientsCommandService>();
builder.Services.AddScoped<PatientsQueryService>();
builder.Services.AddScoped<VaccinationCommandService>();
builder.Services.AddScoped<VaccinationQueryService>();
builder.Services.AddScoped<MedicalRecordsCommandService>();
builder.Services.AddScoped<MedicalRecordsQueryService>();
builder.Services.AddScoped<DemoDataSeeder>();

// ------------------------------------------------------------------- interfaz
builder.Services.AddControllers();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VetPass API",
        Version = "v1",
        Description = "Cartilla de vacunación digital e historial veterinario. " +
                      "Concentra las reglas de negocio de la plataforma: las aplicaciones web y " +
                      "móvil no validan el esquema de vacunación, solo lo presentan."
    });
    options.EnableAnnotations();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token obtenido en /api/v1/authentication/sign-in."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Orígenes que pueden llamar a la API desde un navegador: la aplicación web
// de la clínica. La aplicación móvil no los necesita, porque no es un
// navegador. Sin orígenes configurados, desarrollo admite cualquiera y
// producción ninguno.
var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    else if (builder.Environment.IsDevelopment())
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// ------------------------------------------------------------------ arranque
await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<VetPassDbContext>();

    // Las migraciones y el catálogo de vacunas —datos de la plataforma, no de
    // una clínica— se aplican al arrancar en desarrollo. En producción la API
    // se apaga y vuelve a arrancar con el tráfico, de modo que la base se
    // prepara una sola vez al desplegar (Database:AutoMigrate = false) y cada
    // arranque en frío no repite ese trabajo.
    if (app.Configuration.GetValue("Database:AutoMigrate", true))
    {
        await context.Database.MigrateAsync();
        await VaccinationCatalogSeeder.SeedAsync(context);
    }

    if (app.Configuration.GetValue("Seed:Demo", false))
    {
        var password = app.Configuration["Seed:Password"] ?? "VetPass.2026";
        await services.GetRequiredService<DemoDataSeeder>().SeedAsync(password);
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "VetPass API v1"));
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Responde sin tocar la base: sirve para despertar la API antes de usarla y
// para que un monitor compruebe que está en línea. Informa además la fecha y
// la zona horaria con que la API calcula la cartilla.
app.MapGet("/api/v1/health", (IClinicClock clock) =>
        Results.Ok(new { status = "ok", today = clock.Today, timeZone = ClinicClock.TimeZoneId }))
    .AllowAnonymous()
    .ExcludeFromDescription();

app.Run();
