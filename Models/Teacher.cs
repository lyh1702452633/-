using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string TeacherNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string? Gender { get; set; }
        
        public DateTime? BirthDate { get; set; }
        
        [StringLength(50)]
        public string? Department { get; set; }
        
        [StringLength(50)]
        public string? Title { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        public DateTime HireDate { get; set; } = DateTime.Now;
        
        // 导航属性
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
} 