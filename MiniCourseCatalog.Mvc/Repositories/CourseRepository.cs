using Microsoft.EntityFrameworkCore;
using MiniCourseCatalog.Mvc.Data;
using MiniCourseCatalog.Mvc.Models;
using MiniCourseCatalog.Mvc.Repositories.Interfaces;

namespace MiniCourseCatalog.Mvc.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;

    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Course>> GetAllReadOnlyAsync() =>
        await _context.Courses
            .Include(c => c.CourseCategory)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<Course>> GetAllAsync() =>
        await _context.Courses
            .Include(c => c.CourseCategory)
            .ToListAsync();

    public async Task<Course?> GetByIdReadOnlyAsync(int id) =>
        await _context.Courses
            .Include(c => c.CourseCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Course?> GetByIdAsync(int id) =>
        await _context.Courses
            .Include(c => c.CourseCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task AddAsync(Course course) =>
        await _context.Courses.AddAsync(course);

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    public async Task<bool> ExistsSameClassAsync(string code, string instructor, DateTime startDate) =>
        await _context.Courses.AsNoTracking()
            .AnyAsync(c =>
                c.Code.ToLower() == code.Trim().ToLower() &&
                c.Instructor.ToLower() == instructor.Trim().ToLower() &&
                c.StartDate.Date == startDate.Date);

    public async Task<List<Course>> FilterAsync(int? categoryId, decimal? minFee, decimal? maxFee)
    {
        var query = _context.Courses
            .Include(c => c.CourseCategory)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(c => c.CourseCategoryId == categoryId.Value);

        if (minFee.HasValue)
            query = query.Where(c => c.TuitionFee >= minFee.Value);

        if (maxFee.HasValue)
            query = query.Where(c => c.TuitionFee <= maxFee.Value);

        return await query.OrderBy(c => c.TuitionFee).ToListAsync();
    }
}
