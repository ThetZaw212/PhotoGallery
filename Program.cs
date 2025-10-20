var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Photos");
    options.Conventions.AllowAnonymousToPage("/Login");
});

//Db + Identity (Cookie auth)
builder.Services.AddDbAndIdentityConfig(builder.Configuration);

// MISC Config
builder.Services.AddMiscConfig();

// Core
builder.Services.AddCoreScopedConfig();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
