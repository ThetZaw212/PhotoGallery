namespace PhotoGallery.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDbAndIdentityConfig(this IServiceCollection services,
       IConfiguration configuration)
        {
            // Project DbContext --SQL Server Connection
            _ = services.AddDbContextPool<PhotoGalleryDbContext>((provider, options) =>
            {
                _ = options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // Identity DbContext -- SQL Server Connection
            _ = services.AddDbContextPool<ApplicationDbContext>((provider, options) =>
            {
                _ = options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // For Identity User
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
           .AddEntityFrameworkStores<ApplicationDbContext>()
           .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>  
            {
                options.LoginPath = "/Index";
                options.AccessDeniedPath = "/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            return services;
        }
        public static IServiceCollection AddCoreScopedConfig(this IServiceCollection services)
        {

            //_ = services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
            _ = services.AddScoped<ITokenBuilder, TokenBuilder>();
            _ = services.AddScoped<AuthorizeAttribute>();

            _ = services.AddAntiforgery();

            return services;
        }

        public static IServiceCollection AddMiscConfig(this IServiceCollection services)
        {
            // Add Miscellaneous
            _ = services.AddHttpContextAccessor();

            // API Lower Case Url
            _ = services.AddRouting(options => options.LowercaseUrls = true);

            // Enable Cors
            _ = services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowedCorsOrigins",
                    builder =>
                    {
                        _ = builder
                            .SetIsOriginAllowed(GLOBAL.IsOriginAllowed)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            return services;
        }

        public static IServiceCollection AddJWTAuthConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // Adding JWT Authentication  
            _ = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                // Adding Jwt Bearer  
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidAudience = configuration["JWT:ValidAudience"],
                        ValidIssuer = configuration["JWT:ValidIssuer"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? "")),
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                });

            _ = services.AddAuthorization(options =>
            {

            });

            return services;
        }
    }
}
