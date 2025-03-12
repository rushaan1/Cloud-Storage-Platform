using Cloud_Storage_Platform.CustomModelBinders;
using Cloud_Storage_Platform.Filters;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Core.Services;
using CloudStoragePlatform.Infrastructure.DbContext;
using CloudStoragePlatform.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularLocalhost",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200") // Allow Angular's localhost
                               .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
                               .AllowAnyHeader();
                    });
            });

            builder.Services.AddScoped<IFoldersRepository, FoldersRepository>();
            builder.Services.AddScoped<IFoldersModificationService, FoldersModificationService>();
            builder.Services.AddScoped<IFoldersRetrievalService, FoldersRetrievalService>();
            builder.Services.AddScoped<IFoldersRepository, FoldersRepository>();
            builder.Services.AddScoped<IFilesRepository, FilesRepository>();
            builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();
            builder.Services.AddScoped<IRecentsRepository, RecentsRepository>();
            builder.Services.AddScoped<ISharingRepository, SharingRepository>();
            builder.Services.AddScoped<IModelBinder, AppendToPath>();
            builder.Services.AddScoped<IModelBinder, RemoveInvalidFileFolderNameCharactersBinder>();

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
            app.UseCors("AllowAngularLocalhost");
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

            /*
             * (Should be implemented after adding user accounts) Home Folder (root) must be excluded when returning a list of folders, only needed in 1 action method of FoldersController
             */

            // keep in mind ApplicationDbContext is seeding data with hard coded initial directory path to avoid complexity
            // 

            // Path should always be supplied with single slash \ but swagger's request body for some reason needs \\

            /*
             * Additional Points:
             * Sort reversing (asc & desc) should be handled by client, by default its asc
             */

            // Loader should be shown in the viewer likely as an overlay and refresh button should only be in expanded navigation drawer

            /* TODO Filters
             * Filters that must be used:
             * (being handled by controllers themselves) When receving any request having folder/file path, at its start the initial C:\CloudStoragePlatform\ must be added
             * When sending response having folder/file path, C:\CloudStoragePlatform\ must be removed
             * (Most likely not needed) Not Confirmed (need to be thought of) Making the folders always come first than files regardless of sorting 
             * Use filters for metadata updation
             * (Should be implemented after adding user accounts) Filter for rejecting any modification attempt on home folder
             */

            // Azure blob storage won't require creating new folders explicitly windows file system

            // inject user identifying stuff in constructor and in repository's constructor
            // handle database concurrency
            // do not forget to add encryption, 256-bit AES encryption at rest I need to add at rest and TLS at transit about which I don't need to worry at all as it's default but I can still mention it in project description

            /* VALID PATHS 
             * \home\op
             */
        }
    }
}
