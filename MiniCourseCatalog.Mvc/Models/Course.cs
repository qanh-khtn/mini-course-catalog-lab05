using System.ComponentModel.DataAnnotations;

namespace MiniCourseCatalog.Mvc.Models;

public class Course : IAuditable, ISoftDeletable
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

    // Concurrency token cũ (Lab04): tăng mỗi lần ghi danh để chống "oversell" chỗ ngồi.
    // Vẫn giữ cho luồng Enroll. RowVersion bên dưới phục vụ Edit/AdjustCapacity của Lab05.
    public int Version { get; set; }

    // --- Audit fields (Lab05) ---
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // --- Soft delete fields (Lab05) ---
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // RowVersion phát hiện Last-Save-Wins ở form Edit. SQLite không tự sinh giá trị
    // rowversion nên AppDbContext gán Guid mới mỗi lần Add/Update (xem ApplyAuditAndSoftDelete).
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public int CourseCategoryId { get; set; }
    public CourseCategory CourseCategory { get; set; } = null!;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
