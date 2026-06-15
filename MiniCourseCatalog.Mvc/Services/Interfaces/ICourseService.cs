using MiniCourseCatalog.Mvc.Models;
using MiniCourseCatalog.Mvc.ViewModels;

namespace MiniCourseCatalog.Mvc.Services.Interfaces;

public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(int id);
    Task<CourseStatsViewModel> GetStatsAsync();
    Task AddAsync(Course course);
    Task<List<Course>> SearchAsync(string keyword, string category);
    Task<List<string>> GetCategoryNamesAsync();
    Task<List<CourseCategory>> GetCourseCategoriesAsync();
    Task<bool> ExistsSameClassAsync(string code, string instructor, DateTime startDate);
    Task<List<CourseListItemViewModel>> FilterAsync(int? categoryId, decimal? minFee, decimal? maxFee);
}
