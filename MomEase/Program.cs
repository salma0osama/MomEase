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
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace MomEase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ... (كل الـ using اللي فوق زي ما هي)

            var builder = WebApplication.CreateBuilder(args);

            // 1. إضافة الخدمات الأساسية
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDbContext<MomEaseDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly("MomEase.infra")));

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
            var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();

            // 2. إعداد الـ Authentication
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

            // 3. توحيد سياسة الـ CORS (حل مشكلة الشاشة البيضاء)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true) // بديل آمن لـ AllowAnyOrigin يسمح بالـ Credentials
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // ضروري لـ SignalR
                });
            });

            // 4. تسجيل كل الـ Repositories والـ Services (زي ما هي في كودك)
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
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MomEase API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] {} }
    });
                c.OperationFilter<SwaggerLanguageHeaderFilter>();
            });

            builder.Services.AddSignalR();

            var app = builder.Build();
            // ترتيب الـ Middlewares (مهم جداً)
            app.UseDeveloperExceptionPage(); // نفعله دائماً مؤقتاً عشان نشوف لو فيه خطأ

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // المسار ده هو الأصح والأنسب للـ Local وللسيرفر (بدون نقطة وبدون تكرار)
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PostCare API V1");

                // عشان يفتح معاكي بكلمة swagger زي ما طلبتي
                c.RoutePrefix = "swagger";
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors("AllowAll"); // لازم بعد UseRouting وقبل UseAuthentication

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");

            app.Run();
        }
    }
}
