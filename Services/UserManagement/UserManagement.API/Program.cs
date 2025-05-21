using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserManagement.API.Common.Options;
using UserManagement.API.Infrastructure.Configuration;
using UserManagement.API.Infrastructure.Data;
using UserManagement.API.Infrastructure.Middleware;

namespace UserManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);



            #region Authentication

            builder.Services.Configure<AutenticationOptions>(
               builder.Configuration.GetSection(AutenticationOptions.Name));

            var jwtSettingsSection = builder.Configuration.GetSection(AutenticationOptions.Name);
            var jwtSettings = jwtSettingsSection.Get<AutenticationOptions>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = true,
                       ValidateAudience = true,
                       ValidateLifetime = true,
                       ValidIssuer = jwtSettings!.Issuer,
                       ValidAudience = jwtSettings!.Audience,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey))
                   };

                   options.Events = new JwtBearerEvents
                   {
                       OnChallenge = context =>
                       {
                           context.HandleResponse(); // مهم: جلوگیری از پاسخ پیش‌فرض
                           context.Response.StatusCode = 401;
                           context.Response.ContentType = "application/json";
                           return context.Response.WriteAsync("{\"error\": \"دسترسی غیرمجاز: توکن معتبر نیست یا وجود ندارد.\"}");
                       },
                       OnForbidden = context =>
                       {
                           context.Response.StatusCode = 403;
                           context.Response.ContentType = "application/json";
                           return context.Response.WriteAsync("{\"error\": \"شما اجازه دسترسی به این منبع را ندارید.\"}");
                       }
                   };
               });

            #endregion




            var assembly = typeof(Program).Assembly;

            // تنظیمات قبلی...
            builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("Database");
            builder.Services.ConfigureSqlServer(builder.Configuration);
            builder.Services.ConfigureMediatR(builder.Configuration);
            builder.Services.ConfigureService();
            builder.Services.AddValidatorsFromAssembly(assembly);
            builder.Services.AddCarter();
            builder.Services.AddEndpointsApiExplorer();


            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    policy =>
                    {
                        policy.AllowAnyOrigin() 
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
            });

          

            // تنظیمات بهبود یافته Swagger
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "User Management API",
                    Version = "v1",
                    Description = "API برای مدیریت کاربران سیستم",
                });

                // تعریف احراز هویت با توکن JWT
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.MapType<Guid?>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid",
                    Nullable = true
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
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
                c.UseAllOfForInheritance();
                c.UseOneOfForPolymorphism();
                c.SelectDiscriminatorNameUsing(type => type.Name);                
                c.CustomSchemaIds(type => type.FullName);
                c.EnableAnnotations();
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "UserManagement.API.xml"));
            });

            // ادامه تنظیمات قبلی...
            builder.Services.AddScoped<ITokenService, JwtTokenService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddExceptionHandler<ErrorHandlerMiddleware>();
            builder.Services.AddProblemDetails();
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                    policy.RequireAssertion(_ => true));
            });

            var app = builder.Build();
            app.UseCors("AllowAllOrigins");
            app.UseExceptionHandler();
            app.UseStatusCodePages();

            // تنظیم Swagger
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
                c.SerializeAsV2 = false;
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}" }
        };
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
                c.RoutePrefix = "swagger";
            });

            // ادامه تنظیمات قبلی...
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapCarter();
            app.UseHealthChecks("/health",
               new HealthCheckOptions
               {
                   ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
               });

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                DbInitializer.Seed(db);
            }

            app.Run();
        }
    }
}