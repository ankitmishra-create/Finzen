using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Services;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence;
using FinanceManagement.Infrastructure.Persistence.External;
using FinanceManagement.Infrastructure.Persistence.Repositories;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using FinanceManagement.Infrastructure.Persistence.Seeders;
using FinanceManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection =
            builder.Configuration.GetSection("Authentication:Google");

        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];

        options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
        options.Scope.Add("https://www.googleapis.com/auth/user.gender.read");
    });

builder.Services.AddHttpClient<ICountryApiService, CountryApiService>();
builder.Services.AddHttpClient<ICurrencyConversionService, CurrencyConversionService>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CountrySeeders>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGenerateToken, GenerateToken>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ILoggedInUser, LoggedInUser>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
builder.Services.AddSingleton<IPasswordHashing, PasswordHashing>();

builder.Services.AddAuthentication("MyCookie").AddCookie("MyCookie", opt =>
{
    opt.ExpireTimeSpan = TimeSpan.FromDays(30);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = services.GetRequiredService<CountrySeeders>();
    await seeder.SeedAsync();
}

app.UseMiddleware<GlobalExceptionHandling>();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();