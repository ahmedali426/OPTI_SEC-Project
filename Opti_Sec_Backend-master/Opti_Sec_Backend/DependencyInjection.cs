using System.Reflection;
using System.Text;
using FluentValidation;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Opti_Sec_Backend.Authentications;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Helper;
using Opti_Sec_Backend.Persistence;
using Opti_Sec_Backend.Services.AccessLogServices;
using Opti_Sec_Backend.Services.AuthServices;
using Opti_Sec_Backend.Services.ClientServices;
using Opti_Sec_Backend.Services.EmailServices;
using Opti_Sec_Backend.Services.FileServices;
using Opti_Sec_Backend.Services.GateServices;
using Opti_Sec_Backend.Services.MemberServices;
using Opti_Sec_Backend.Services.RoleServices;
using Opti_Sec_Backend.Services.DeviceCommandServices;
using Opti_Sec_Backend.Services.EmergencyServices;
using Opti_Sec_Backend.Services.FingerprintServices;
using Opti_Sec_Backend.Services.NotificationServices;
using Opti_Sec_Backend.Services.PasswordServices;
using Opti_Sec_Backend.Services.SecurityWorkflow;
using Opti_Sec_Backend.Services.SessionServices;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Polly;
using Polly.Extensions.Http;
using Opti_Sec_Backend.Settings;
using Opti_Sec_Backend.Services.AIServices;

namespace Opti_Sec_Backend;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,IConfiguration configuration)
    {
        // Add services to the container.
        services.AddControllers();
        // Add Swagger 
        services.AddSwaggerServices();
        // to define mapster 
        services.AddMapsterServices();
        // to add connectionstring
        services.AddConnectionString(configuration);
        // to define fluent validation 
        services.AddFluentValidationServices();
        // to add auth services
        services.AddAuthService(configuration);
        // to add background jobs
        services.AddBackgroundJobsConfig(configuration);


        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IGateService, GateService>();
        services.AddScoped<IAccessLogService, AccessLogService>();

        // Security Workflow Services
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmergencyService, EmergencyService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDeviceCommandService, DeviceCommandService>();
        services.AddScoped<IFingerprintService, FingerprintService>();
        services.AddScoped<IGateAccessOrchestrator, GateAccessOrchestrator>();
        services.AddScoped<SessionCleanupJob>();

        // inject services here 
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<EmailHelper>();

        // SignalR
        services.AddSignalR();


        services.Configure<MailSettings>(
    configuration.GetSection("MailSettings"));

        // AI Training Service configuration & typed HttpClient with Polly retry policy
        services.Configure<AIServiceSettings>(configuration.GetSection(AIServiceSettings.SectionName));
        services.AddHttpClient<IAITrainingService, AITrainingService>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<AIServiceSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl);
            if (!string.IsNullOrEmpty(settings.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-KEY", settings.ApiKey);
            }
        })
        .AddPolicyHandler(GetRetryPolicy());

        // to add the Exception handler 
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();


        return services;
    }

    private static IServiceCollection AddMapsterServices(this IServiceCollection services)
    {
        var mappingCongfig = TypeAdapterConfig.GlobalSettings;
        mappingCongfig.Scan(Assembly.GetExecutingAssembly());
        return services;
    }
    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter JWT Token like this: Bearer {your token}"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
            });
        });

        return services;
    }
    private static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services,
           IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        services.AddHangfireServer();

        return services;
    }
    private static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        services
             .AddFluentValidationAutoValidation()
             .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    private static IServiceCollection AddConnectionString(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("connection string 'DefaultConnection' not found ");

        services.AddDbContext<ApplicationDbContext>(option =>
        option.UseSqlServer(connectionString));

        //services.AddIdentity<ApplicationUser, IdentityRole>()
        //    .AddEntityFrameworkStores<ApplicationDbContext>()
        //    .AddDefaultTokenProviders();

        //services.AddIdentity<ApplicationUser, ApplicationRole>()
        //.AddEntityFrameworkStores<ApplicationDbContext>()
        //.AddDefaultTokenProviders();

        return services;
    }
    private static IServiceCollection AddAuthService(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddSingleton<IJwtProvider,JwtProvider>();

        //services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

        services.AddOptions<JwtOptions>()
         .BindConfiguration(JwtOptions.SectionName)
         .ValidateDataAnnotations()
         .ValidateOnStart();

        services.AddIdentity<ApplicationUser, ApplicationRole>()
          .AddEntityFrameworkStores<ApplicationDbContext>()
          .AddDefaultTokenProviders();

        services.AddAuthentication(option =>
        {
            // to define the default authentication schema 
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
       .AddJwtBearer(o =>
       {
           // to  make the token valid during the expiration mean you can 
           // arrive to it 
           o.SaveToken = true;
           o.TokenValidationParameters = new TokenValidationParameters
           {
               // to check the key valid or not 
               ValidateIssuerSigningKey = true,
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               // now we must define to it what the valid data 
               IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
               ValidIssuer = jwtSettings?.Issuer,
               ValidAudience = jwtSettings?.Audience,
           };
       });

        // to make the configuration on the Identity like as the Password 
        services.Configure<IdentityOptions>(option =>
        {
            option.Password.RequireDigit = true;
            option.Password.RequireUppercase = true;
            option.Password.RequireLowercase = true;
            option.Password.RequireNonAlphanumeric = true;
            option.Password.RequiredLength = 8;
            // lockout user configuration
            option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            option.Lockout.MaxFailedAccessAttempts = 5;
            option.Lockout.AllowedForNewUsers = true;
        });

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
