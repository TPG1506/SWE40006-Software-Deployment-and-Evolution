using System.ComponentModel.DataAnnotations;

namespace TaskTracker.Models;

public class TaskItem
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    public bool IsDone { get; set; }
}