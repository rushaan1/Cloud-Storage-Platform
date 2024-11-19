using CloudStoragePlatform.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CloudStoragePlatform.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string defaultDirectory = builder.Configuration.GetValue<string>("InitialPathForStorage");
            if (!Directory.Exists(defaultDirectory)) 
            {
                Directory.CreateDirectory(Path.Combine(defaultDirectory, "home"));
            }

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


            /*
             * Additional Points:
             * Sort reversing (asc & desc) should be handled by client, by default its asc
             */


            /* TODO Filters
             * Filters that must be used:
             * When receving creation/rename request that includes a folder/file path all invalid folder name characters must be removed
             * When receving any request having folder/file path, at its start the initial C:\CloudStoragePlatform\ must be added
             * When sending response having folder/file path, C:\CloudStoragePlatform\ must be removed
             * Not Confirmed (need to be thought of) Making the folders always come first than files regardless of sorting 
             * Home Folder (root) must be excluded when returning a list of folders
             * Use filters for metadata updation
             */
        }
    }
}
