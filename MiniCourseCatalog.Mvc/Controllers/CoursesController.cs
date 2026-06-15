using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniCourseCatalog.Mvc.Models;
using MiniCourseCatalog.Mvc.Services.Interfaces;
using MiniCourseCatalog.Mvc.ViewModels;

namespace MiniCourseCatalog.Mvc.Controllers;

public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IStudentService _studentService;

    public CoursesController(
        ICourseService courseService,
        IEnrollmentService enrollmentService,
        IStudentService studentService)
    {
        _courseService = courseService;
        _enrollmentService = enrollmentService;
        _studentService = studentService;
    }

    public async Task<IActionResult> Index(string keyword = "", string category = "", string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var rawCourses = await _courseService.GetAllAsync();
        var categories = rawCourses
            .Select(c => c.CourseCategory.Name)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var filtered = rawCourses.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(keyword))
            filtered = filtered.Where(c =>
                c.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                c.Instructor.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(category))
            filtered = filtered.Where(c =>
                string.Equals(c.CourseCategory.Name, category, StringComparison.OrdinalIgnoreCase));

        var courseItems = filtered.Select(c => new CourseListItemViewModel
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Category = c.CourseCategory.Name,
            Instructor = c.Instructor,
            TuitionFee = c.TuitionFee,
            CurrentEnrollment = c.CurrentEnrollment,
            MaxCapacity = c.MaxCapacity
        }).ToList();

        var viewModel = new CourseIndexViewModel
        {
            Courses = courseItems,
            Categories = categories,
            Keyword = keyword,
            Category = category,
            Theme = theme,
            TotalCoursesBeforeFilter = rawCourses.Count
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Detail(int id, string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var course = await _courseService.GetByIdAsync(id);
        if (course == null)
            return NotFound($"Không thể tìm thấy thông tin khóa học với mã ID = {id}");

        var detailVm = new CourseDetailViewModel
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Category = course.CourseCategory.Name,
            Instructor = course.Instructor,
            TuitionFee = course.TuitionFee,
            CurrentEnrollment = course.CurrentEnrollment,
            MaxCapacity = course.MaxCapacity,
            StartDate = course.StartDate
        };

        return View(detailVm);
    }

    public async Task<IActionResult> Stats(string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var statsVm = await _courseService.GetStatsAsync();
        return View(statsVm);
    }

    [HttpGet]
    public async Task<IActionResult> Filter(int? categoryId, decimal? minFee, decimal? maxFee, string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var categories = await _courseService.GetCourseCategoriesAsync();
        var vm = new CourseFilterViewModel
        {
            CategoryId = categoryId,
            MinFee = minFee,
            MaxFee = maxFee,
            Theme = theme,
            Categories = categories,
            HasSearched = categoryId.HasValue || minFee.HasValue || maxFee.HasValue
        };

        if (vm.HasSearched)
            vm.Results = await _courseService.FilterAsync(categoryId, minFee, maxFee);

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string keyword = "", string category = "", string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var results = (await _courseService.SearchAsync(keyword, category))
            .Select(c => new CourseListItemViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Category = c.CourseCategory.Name,
                Instructor = c.Instructor,
                TuitionFee = c.TuitionFee,
                CurrentEnrollment = c.CurrentEnrollment,
                MaxCapacity = c.MaxCapacity
            })
            .ToList();

        var viewModel = new CourseSearchViewModel
        {
            Keyword = keyword,
            Category = category,
            Theme = theme,
            Categories = await _courseService.GetCategoryNamesAsync(),
            Results = results
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var categories = await _courseService.GetCourseCategoriesAsync();
        var viewModel = new CourseCreateViewModel
        {
            StartDate = DateTime.Today,
            MaxCapacity = 20,
            CategoryOptions = categories
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseCreateViewModel viewModel, string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        if (await _courseService.ExistsSameClassAsync(viewModel.Code, viewModel.Instructor, viewModel.StartDate))
        {
            ModelState.AddModelError(
                nameof(viewModel.Code),
                "Lớp học này đã tồn tại với cùng mã khóa học, giảng viên và ngày khai giảng.");
        }

        if (!ModelState.IsValid)
        {
            var categories = await _courseService.GetCourseCategoriesAsync();
            viewModel.CategoryOptions = categories
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
                .ToList();
            return View(viewModel);
        }

        var course = new Course
        {
            Code = viewModel.Code,
            Name = viewModel.Name,
            CourseCategoryId = viewModel.CourseCategoryId,
            Instructor = viewModel.Instructor,
            TuitionFee = viewModel.TuitionFee,
            CurrentEnrollment = viewModel.CurrentEnrollment,
            MaxCapacity = viewModel.MaxCapacity,
            StartDate = viewModel.StartDate
        };

        await _courseService.AddAsync(course);
        TempData["SuccessMessage"] = $"Đã thêm khóa học '{course.Name}' thành công.";
        return RedirectToAction(nameof(Index), new { theme });
    }

    [HttpGet]
    public async Task<IActionResult> Enroll(string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        var vm = await BuildEnrollViewModelAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(EnrollViewModel viewModel, string theme = "light")
    {
        theme = NormalizeTheme(theme);
        ViewData["Theme"] = theme;

        if (!ModelState.IsValid)
        {
            var fresh = await BuildEnrollViewModelAsync();
            fresh.CourseId = viewModel.CourseId;
            fresh.StudentId = viewModel.StudentId;
            return View(fresh);
        }

        var (success, message) = await _enrollmentService.EnrollStudentAsync(viewModel.CourseId, viewModel.StudentId);

        if (success)
        {
            // PRG: redirect sau khi ghi thành công — toast hiện ở trang mới, F5 không submit lại form
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Enroll), new { theme });
        }

        // Thất bại (hết chỗ / trùng / lỗi concurrency): giữ thông báo inline trên form
        var vm = await BuildEnrollViewModelAsync();
        vm.CourseId = viewModel.CourseId;
        vm.StudentId = viewModel.StudentId;
        vm.IsSuccess = success;
        vm.ResultMessage = message;

        return View(vm);
    }

    public IActionResult Welcome() =>
        Content("Hệ thống quản lý đào tạo Mini Training Center xin chào học viên!");

    public async Task<IActionResult> CourseJson() =>
        Json(await _courseService.GetAllAsync());

    public IActionResult GoToList() =>
        RedirectToAction(nameof(Index));

    public IActionResult Force404() =>
        NotFound("Đây là trang phản hồi mẫu 404 thử nghiệm từ hệ thống.");

    public IActionResult CategoryInfo() =>
        Content("Xem danh mục tại /DataHealth");

    private static string NormalizeTheme(string theme) =>
        string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase) ? "dark" : "light";

    private async Task<EnrollViewModel> BuildEnrollViewModelAsync()
    {
        var courses = await _courseService.GetAllAsync();
        var students = await _studentService.GetAllAsync();
        return new EnrollViewModel
        {
            CourseOptions = courses
                .Select(c => new SelectListItem($"{c.Code} – {c.Name} ({c.CurrentEnrollment}/{c.MaxCapacity})", c.Id.ToString()))
                .ToList(),
            StudentOptions = students
                .Select(s => new SelectListItem($"{s.FullName} ({s.Email})", s.Id.ToString()))
                .ToList()
        };
    }
}
