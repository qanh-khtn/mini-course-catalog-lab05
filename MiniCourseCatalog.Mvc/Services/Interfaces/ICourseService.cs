using MiniCourseCatalog.Mvc.Models;
using MiniCourseCatalog.Mvc.ViewModels;

namespace MiniCourseCatalog.Mvc.Services.Interfaces;

/// <summary>Kết quả thao tác cập nhật khóa học (dùng cho Edit có RowVersion).</summary>
public enum CourseUpdateResult
{
    Success,
    NotFound,
    ConcurrencyConflict
}

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

    // --- Lab05: CRUD an toàn + soft delete ---
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);
    Task<CourseEditViewModel?> GetForEditAsync(int id);
    Task<CourseDeleteViewModel?> GetForDeleteAsync(int id);
    Task<CourseUpdateResult> UpdateAsync(CourseEditViewModel viewModel);
    Task<bool> SoftDeleteAsync(int id);
    Task<List<CourseTrashItemViewModel>> GetTrashAsync();
    Task<bool> RestoreAsync(int id);
}
