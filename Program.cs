using System;
using System.Text.Json.Serialization;
using TagerCom.Services;
using System.Text.Json.Serialization;

namespace TagerCom
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Optional: Keep OpenAPI (generates JSON)
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(option =>
            {
                option.Password.RequiredLength = 8;      // Minimum password length
                option.User.RequireUniqueEmail = true;   // Require unique email per user
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/api/Identity/Accounts/Login";
                options.AccessDeniedPath = "/api/Identity/Accounts/AccessDenied";
            });
            builder.Services.AddScoped<TokenService>();
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Generic repository pattern
            builder.Services.AddScoped<IUserRepository, UserRepository>(); // User repository 
            builder.Services.AddMemoryCache();


            // Add Swagger UI support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TagerCom API",
                    Version = "v1",
                    Description = "API documentation for TagerCom project"
                });
            });


            builder.Services
                .AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            var app = builder.Build();
            // --------------------------------------------------------
            // DbInitializer
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                dbInitializer.Initialize();
            }
            // --------------------------------------------------------
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // This generates openapi.json
                app.MapOpenApi();

                //  Enable Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TagerCom API v1");
                    options.RoutePrefix = "swagger"; // open at /swagger
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}