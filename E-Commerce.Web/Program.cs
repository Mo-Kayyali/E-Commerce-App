using DomainLayer.Contracts;
using DomainLayer.Models.IdentityModule;
using E_Commerce.Web.CustomMiddleWares;
using E_Commerce.Web.Extensions;
using E_Commerce.Web.Factories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Data;
using Persistence.Repositories;
using Service;
using Service.MappingProfiles;
using ServiceAbstraction;
using Shared.ErrorModels;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json;


namespace E_Commerce.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            builder.Services.AddCors(Options =>
            {
                Options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200");
                } );
            });
            builder.Services.AddSwaggerServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddWebApplicationServices();
            builder.Services.AddJWTService(builder.Configuration);


            var app = builder.Build();


            await app.SeedDataBaseAsync();

            app.UseCustomExceptionMiddleWare();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(Options =>
                {
                    Options.ConfigObject = new ConfigObject()
                    {
                        DisplayRequestDuration = true
                    };

                    Options.DocumentTitle = "My E-Commerce API";

                    Options.JsonSerializerOptions = new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy=JsonNamingPolicy.CamelCase
                    };

                    Options.DocExpansion(DocExpansion.None);

                    Options.EnableFilter();

                    Options.EnablePersistAuthorization();
                });
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
