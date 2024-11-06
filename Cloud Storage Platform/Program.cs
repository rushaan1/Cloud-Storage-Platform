using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CloudStoragePlatform.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
                options.UseLazyLoadingProxies();
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

            /*
             * TODO services: Add file, remove file, edit file name, replace file, get all contents with main folder, get specific files from specific folder, sorting, filtering, move files, favorite, unfavorite, share functionality
             */
        }
    }
}
