using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Swashbuckle.AspNetCore.Swagger;

namespace TagerCom
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // ? Optional: Keep OpenAPI (generates JSON)
            builder.Services.AddOpenApi();

            // ? Add Swagger UI support
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

            var app = builder.Build();

            // ? Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // This generates openapi.json
                app.MapOpenApi();

                // ? Enable Swagger UI
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TagerCom API v1");
                    options.RoutePrefix = "swagger"; // open at /swagger
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
