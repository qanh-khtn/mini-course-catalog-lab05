using Microsoft.EntityFrameworkCore;
using MiniCourseCatalog.Mvc.Data;
using MiniCourseCatalog.Mvc.Filters;
using MiniCourseCatalog.Mvc.Options;
using MiniCourseCatalog.Mvc.Repositories;
using MiniCourseCatalog.Mvc.Repositories.Interfaces;
using MiniCourseCatalog.Mvc.Services;
using MiniCourseCatalog.Mvc.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new ThemeFilter());
});

// Options Pattern — validate bằng Data Annotation ngay khi khởi động:
// config sai (vd LowSeatThreshold = -5) thì app từ chối chạy thay vì lỗi ngầm lúc runtime
builder.Services.AddOptions<TrainingCenterConfig>()
    .Bind(builder.Configuration.GetSection(TrainingCenterConfig.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// EF Core + SQLite (Scoped by default)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories — Scoped
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();

// Services — Scoped
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
