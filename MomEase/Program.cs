using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MomEase.api.Filters;
using MomEase.infra.Hubs;
using MomEase.core.Entities;
using MomEase.core.Interfaces;
using MomEase.core.Repositories;
using MomEase.infra.Data;
using MomEase.infra.Repositories;
using MomEase.infra.Seeders;
using MomEase.infra.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace MomEase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Services.AddHttpContextAccessor();

                builder.Services.AddDbContext<MomEaseDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        b => b.MigrationsAssembly("MomEase.infra")));

                builder.Services.Configure<JwtSettings>(
                    builder.Configuration.GetSection("JWT"));
                var jwtSettings = builder.Configuration
                    .GetSection("JWT").Get<JwtSettings>();

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
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
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

                builder.Services.AddAuthorization();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.SetIsOriginAllowed(_ => true)
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                });

                builder.Services.AddScoped<IJwtService, JwtService>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<IAuthRepository, AuthRepository>();
                builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IMotherProfileRepository, MotherProfileRepository>();
                builder.Services.AddScoped<IMotherProfileService, MotherProfileService>();
                builder.Services.AddScoped<IChildRepository, ChildRepository>();
                builder.Services.AddScoped<IChildService, ChildService>();
                builder.Services.AddScoped<IFileStorageService, FileStorageService>();
                builder.Services.AddScoped<IGrowthRecordRepository, GrowthRecordRepository>();
                builder.Services.AddScoped<IGrowthRecordService, GrowthRecordService>();
                builder.Services.AddScoped<IGrowthPercentileReferenceRepository, GrowthPercentileReferenceRepository>();
                builder.Services.AddScoped<IFeedingReferenceRepository, FeedingReferenceRepository>();
                builder.Services.AddScoped<IFeedingRecordRepository, FeedingRecordRepository>();
                builder.Services.AddScoped<IFeedingRecordService, FeedingRecordService>();
                builder.Services.AddScoped<ISleepRecordRepository, SleepRecordRepository>();
                builder.Services.AddScoped<ISleepRecordService, SleepRecordService>();
                builder.Services.AddScoped<ISleepReferenceRepository, SleepReferenceRepository>();
                builder.Services.AddScoped<IGrowthReportRepository, GrowthReportRepository>();
                builder.Services.AddScoped<IGrowthReportService, GrowthReportService>();
                builder.Services.AddScoped<IChatBotRepository, ChatBotRepository>();
                builder.Services.AddScoped<IChatBotService, ChatBotService>();
                builder.Services.AddHttpClient<ILlamaService, LlamaService>();
                builder.Services.AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>();
                builder.Services.AddScoped<IArticleCategoryService, ArticleCategoryService>();
                builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
                builder.Services.AddScoped<IArticleService, ArticleService>();
                builder.Services.AddScoped<ISearchHistoryService, SearchHistoryService>();
                builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
                builder.Services.AddScoped<ISavedArticleService, SavedArticleService>();
                builder.Services.AddScoped<ISavedArticleRepository, SavedArticleRepository>();
                builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
                builder.Services.AddScoped<IAssessmentService, AssessmentService>();
                builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
                builder.Services.AddScoped<IQuestionService, QuestionService>();
                builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
                builder.Services.AddScoped<IAnswerOptionService, AnswerOptionService>();
                builder.Services.AddScoped<IScoreLevelRepository, ScoreLevelRepository>();
                builder.Services.AddScoped<IScoreLevelService, ScoreLevelService>();
                builder.Services.AddScoped<IAssessmentResultRepository, AssessmentResultRepository>();
                builder.Services.AddScoped<IUserResponseRepository, UserResponseRepository>();
                builder.Services.AddScoped<IUserResponseService, UserResponseService>();
                builder.Services.AddScoped<ISkinAnalysisRepository, SkinAnalysisRepository>();
                builder.Services.AddScoped<IDiseaseRepository, DiseaseRepository>();
                builder.Services.AddScoped<ISkinAnalysisService, SkinAnalysisService>();
                builder.Services.AddScoped<IVaccinationRepository, VaccinationRepository>();
                builder.Services.AddScoped<IVaccinationService, VaccinationService>();
                builder.Services.AddScoped<ISkinAnalysisAIService, SkinAnalysisAIService>();
                builder.Services.AddHttpClient();
                builder.Services.AddScoped<ICommunityRepository, CommunityRepository>();
                builder.Services.AddScoped<ICommunityService, CommunityService>();
                builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
                builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
                builder.Services.AddScoped<INotificationService, NotificationService>();
                builder.Services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
                builder.Services.AddScoped<IMentalHealthFollowUpRepository, MentalHealthFollowUpRepository>();
                builder.Services.AddScoped<IMentalHealthTipRepository, MentalHealthTipRepository>();
                builder.Services.AddScoped<IMentalHealthFollowUpService, MentalHealthFollowUpService>();
                builder.Services.AddHostedService<MentalHealthFollowUpBackgroundService>();
                builder.Services.AddScoped<IAssessmentResultService, AssessmentResultService>();

                builder.Services.AddControllers().AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter());
                });

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.OperationFilter<SwaggerLanguageHeaderFilter>();
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "MomEase API",
                        Version = "v1"
                    });
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme.",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = "Bearer"
                    });
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    });
                });

                builder.Services.AddSignalR();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider
                        .GetRequiredService<MomEaseDbContext>();
                    AssessmentSeeder.SeedAsync(context).GetAwaiter().GetResult();
                }

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MomEase API v1");
                    c.RoutePrefix = "swagger";
                });

                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRouting();
                app.UseCors("AllowAll");
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();
                app.MapHub<NotificationHub>("/notificationHub");
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}