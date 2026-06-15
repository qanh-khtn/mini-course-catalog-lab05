using MiniCourseCatalog.Mvc.Models;

namespace MiniCourseCatalog.Mvc.Repositories.Interfaces;

public interface ICourseRepository
{
    Task<List<Course>> GetAllReadOnlyAsync();
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByIdReadOnlyAsync(int id);
    Task<Course?> GetByIdAsync(int id);
    Task AddAsync(Course course);
    Task SaveChangesAsync();
    Task<bool> ExistsSameClassAsync(string code, string instructor, DateTime startDate);
    Task<List<Course>> FilterAsync(int? categoryId, decimal? minFee, decimal? maxFee);
}
