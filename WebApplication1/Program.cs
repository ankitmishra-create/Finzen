using FinanceManagement.Application.Interfaces;
using FinanceManagement.Application.Services;
using FinanceManagement.Infrastructure.Interface;
using FinanceManagement.Infrastructure.Persistence;
using FinanceManagement.Infrastructure.Persistence.Repositories;
using FinanceManagement.Infrastructure.Persistence.Repositories.InterfaceRepository;
using FinanceManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHashing, PasswordHashing>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGenerateToken, GenerateToken>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ILoggedInUser, LoggedInUser>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddAuthentication("MyCookie").AddCookie("MyCookie", opt =>
{
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
