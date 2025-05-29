using Etalem.Data;
using Etalem.Data.Repo;
using Etalem.Data.Repo.Interfaces;
using Etalem.Infrastructure.Services;
using Etalem.MappingProfiles;
using Etalem.Models;
using Etalem.Models.DTOs.Course;
using Etalem.Services;
using Etalem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



// إعداد Identity مع الأدوار
builder.Services.AddIdentity<IdentityUser ,IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; 
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI() // إضافة واجهة Identity الافتراضية
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
});



builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonResourceRepository, LessonResourceRepository>();
builder.Services.AddScoped<IDiscussionRepository, DiscussionRepository>();

//builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<LessonService>();
builder.Services.AddScoped<LessonResourceService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<QuizAttemptService>();
builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<Etalem.Services.ReviewService>();
builder.Services.AddScoped<CertificateGenerationService>();
builder.Services.AddScoped<IFileService, Etalem.Services.FileService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));




// إضافة خدمات Razor Pages و MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();



// تهيئة الأدوار وحساب Admin
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await RoleInitializer.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // لخدمة الملفات الثابتة


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
