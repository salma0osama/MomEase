using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PostCare.core.Entities;
using PostCare.core.Interfaces;
using PostCare.infra.Data;
using PostCare.infra.Repositories;
using PostCare.infra.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace PostCare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add DbContext
            builder.Services.AddDbContext<PostCareDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("PostCare.infra")
                )
            );
            // Configure JWT Settings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
            var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();

            // Add Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();



            // Add services to the container.

            //Authentication Repository & Service
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthRepository, AuthRepository>();
            builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // user Repository & Service
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Mother Profile Repository & Service
            builder.Services.AddScoped<IMotherProfileRepository, MotherProfileRepository>();
            builder.Services.AddScoped<IMotherProfileService, MotherProfileService>();

            // Child Repository & Service
            builder.Services.AddScoped<IChildRepository, ChildRepository>();
            builder.Services.AddScoped<IChildService, ChildService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();

            // Feeding Record Repository & Service
            builder.Services.AddScoped<IFeedingReferenceRepository, FeedingReferenceRepository>();
            builder.Services.AddScoped<IFeedingRecordRepository, FeedingRecordRepository>();

            // Feeding Tracking Services
            builder.Services.AddScoped<IFeedingRecordService, FeedingRecordService>();


            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                // ✅ تحويل كل الـ Enums لـ strings
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            //sleep record Repository & Service
            builder.Services.AddScoped<ISleepRecordRepository, SleepRecordRepository>();
            builder.Services.AddScoped<ISleepRecordService, SleepRecordService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            // Add Swagger with JWT Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PostCare API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
            new OpenApiSecurityScheme
              {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
             },
            new string[] {}
             }
             });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
