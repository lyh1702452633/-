using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        
        public DateTime AttendanceDate { get; set; }
        
        [Required]
        public AttendanceStatus Status { get; set; }
        
        [StringLength(200)]
        public string? Remarks { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // 导航属性
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
    
    public enum AttendanceStatus
    {
        Present = 1,    // 出勤
        Absent = 2,     // 缺勤
        Late = 3,       // 迟到
        Leave = 4       // 请假
    }
} 