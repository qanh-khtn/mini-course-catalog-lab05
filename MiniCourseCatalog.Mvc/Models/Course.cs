using System.ComponentModel.DataAnnotations;

namespace MiniCourseCatalog.Mvc.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string Code { get; set; } = "";

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string Instructor { get; set; } = "";

    public decimal TuitionFee { get; set; }
    public int CurrentEnrollment { get; set; }
    public int MaxCapacity { get; set; }
    public DateTime StartDate { get; set; }

    // Concurrency token: tăng mỗi lần cập nhật sĩ số; 2 request cùng đọc một Version
    // thì chỉ request commit trước thắng, request sau nhận DbUpdateConcurrencyException
    public int Version { get; set; }

    public int CourseCategoryId { get; set; }
    public CourseCategory CourseCategory { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
