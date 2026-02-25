using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.infra.Data;
using MomEase.infra.Repositories;
using MomEase.infra.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace MomEase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add DbContext
            builder.Services.AddDbContext<MomEaseDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("MomEase.infra")
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

            // Growth Record Repository & Service
            builder.Services.AddScoped<IGrowthRecordRepository, GrowthRecordRepository>();
            builder.Services.AddScoped<IGrowthRecordService, GrowthRecordService>();
            // Feeding Record Repository & Service
            builder.Services.AddScoped<IFeedingReferenceRepository, FeedingReferenceRepository>();
            builder.Services.AddScoped<IFeedingRecordRepository, FeedingRecordRepository>();

            // Feeding Tracking Services
            builder.Services.AddScoped<IFeedingRecordService, FeedingRecordService>();

            //sleep record Repository & Service
            builder.Services.AddScoped<ISleepRecordRepository, SleepRecordRepository>();
            builder.Services.AddScoped<ISleepRecordService, SleepRecordService>();

            // Sleep Reference Repository
            builder.Services.AddScoped<ISleepReferenceRepository, SleepReferenceRepository>();

            // ChatBot Services
            builder.Services.AddScoped<IChatBotRepository, ChatBotRepository>();
            builder.Services.AddScoped<IChatBotService, ChatBotService>();
            builder.Services.AddHttpClient<ILlamaService, LlamaService>();

            // Article Category Repository & Service
            builder.Services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
            builder.Services.AddScoped<IArticleCategoryService, ArticleCategoryService>();
            
            // Article Repository & Service
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();
            builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
            builder.Services.AddScoped<ISavedArticleService, SavedArticleService>();
            builder.Services.AddScoped<ISavedArticleRepository, SavedArticleRepository>();

            // Assessment Repository & Service
            builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
            builder.Services.AddScoped<IAssessmentService, AssessmentService>();

            // Question Repository & Service
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();

            // Answer Option Repository & Service
            builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
            builder.Services.AddScoped<IAnswerOptionService, AnswerOptionService>();

            // Skin Analysis Services
            builder.Services.AddScoped<ISkinAnalysisRepository, SkinAnalysisRepository>();
            builder.Services.AddScoped<IDiseaseRepository, DiseaseRepository>();
            builder.Services.AddScoped<ISkinAnalysisService, SkinAnalysisService>();

            builder.Services.AddScoped<ISkinAnalysisAIService, SkinAnalysisAIService>();
            builder.Services.AddHttpClient();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                // ✅ تحويل كل الـ Enums لـ strings
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();

            // Add Swagger with JWT Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MomEase API", Version = "v1" });

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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyMethod()
                                      .AllowAnyHeader());
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
