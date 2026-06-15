namespace MiniCourseCatalog.Mvc.Models;

/// <summary>
/// Entity có audit vòng đời dữ liệu. AppDbContext.SaveChangesAsync tự gán
/// CreatedAt khi thêm mới và UpdatedAt khi cập nhật — controller/service không cần set tay.
/// </summary>
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}
