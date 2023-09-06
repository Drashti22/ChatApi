using Chat_Api.Context;
using Chat_Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options=>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            }) ;

            options.OperationFilter<SecurityRequirementsOperationFilter>();

        });
    
        builder.Services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnectionString"));
                });
        //configure Cors
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("SwaggerPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
        });


        builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("This is my 128 bits very long secret key.......")),
                        ValidateAudience = false,
                        ValidateIssuer = false
                    };
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }



                app.UseHttpsRedirection();

                app.UseCors("SwaggerPolicy");

                app.UseAuthentication();

                app.UseAuthorization();

                app.MapControllers();

                app.UseMiddleware<RequestLogginMiddleware>();
            

                app.Run();
            }
}

