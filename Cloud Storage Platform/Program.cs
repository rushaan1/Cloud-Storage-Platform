using Azure.Identity;
using Cloud_Storage_Platform.CustomModelBinders;
using Cloud_Storage_Platform.Filters;
using CloudStoragePlatform.Core;
using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Core.Services;
using CloudStoragePlatform.Infrastructure.DbContext;
using CloudStoragePlatform.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Cloud_Storage_Platform;

namespace CloudStoragePlatform.Web
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = long.MaxValue;
            });
            //builder.WebHost.UseUrls("http://*:80");
            string defaultDirectory = builder.Configuration.GetValue<string>("InitialPathForStorage");
            if (!Directory.Exists(defaultDirectory)) 
            {
                Directory.CreateDirectory(Path.Combine(defaultDirectory, "home"));
            }

            builder.Services.AddControllers(options => 
            {
                var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });
            builder.Services.AddTransient<IJwtService, JwtService>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularLocalhost",
                    builder =>
                    {
                        builder.WithOrigins("https://localhost:4200", "https://192.168.29.161:4200")
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });

            builder.Services.AddSingleton<TokenCredential, DefaultAzureCredential>();
            builder.Services.AddScoped<UserIdentification>();
            builder.Services.AddScoped<IdentifyUser>();
            builder.Services.AddScoped<ThumbnailService>();
            builder.Services.AddScoped<IBulkRetrievalService, BulkRetrievalService>();
            builder.Services.AddScoped<IFoldersRepository, FoldersRepository>();
            builder.Services.AddScoped<IUserSessionsRepository, UserSessionsRepository>();
            builder.Services.AddSingleton<SSE, SSE>();
            builder.Services.AddScoped<IFoldersModificationService, FoldersModificationService>();
            builder.Services.AddScoped<IFoldersRetrievalService, FoldersRetrievalService>();
            builder.Services.AddScoped<IFoldersRepository, FoldersRepository>();
            builder.Services.AddScoped<IFilesRepository, FilesRepository>();
            builder.Services.AddScoped<IFilesRetrievalService, FileRetrievalService>();
            builder.Services.AddScoped<IFilesModificationService, FileModificationService>();
            builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();
            builder.Services.AddScoped<ISharingRepository, SharingRepository>();
            builder.Services.AddScoped<ISharingService, SharingService>();
            builder.Services.AddScoped<IModelBinder, AppendToPath>();
            builder.Services.AddScoped<IModelBinder, RemoveInvalidFileFolderNameCharactersBinder>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
                options.UseLazyLoadingProxies();
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.Configure<KestrelServerOptions>(opts =>
            {
                opts.AllowSynchronousIO = true;
            });

            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
                .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Read JWT from cookie
                            context.Token = context.Request.Cookies["access_token"];
                            return Task.CompletedTask;
                        }
                    };
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtCloudStorageWebApi"]))
                    };
                });

            builder.Services.AddAuthorization(options => { });


            var app = builder.Build();
            
            // Add startup logging
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Application starting up...");
            logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
            logger.LogInformation($"Content Root: {app.Environment.ContentRootPath}");
            
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();
            // Use appropriate CORS policy based on environment
            if (app.Environment.IsDevelopment())
            {
                app.UseCors("AllowAngularLocalhost");
            }
            else
            {
                app.UseCors("AllowAzureAppService");
            }
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
           
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api/Folders/sse"))
                {
                    context.Request.Protocol = "HTTP/2";
                }
                await next();
            });

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
