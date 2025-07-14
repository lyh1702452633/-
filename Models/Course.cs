using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = string.Empty;
        
        public int Credits { get; set; }
        
        [ForeignKey("Teacher")]
        public int TeacherId { get; set; }
        
        [StringLength(200)]
        public string? Description { get; set; }
        
        // 导航属性
        public virtual Teacher Teacher { get; set; } = null!;
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
} 