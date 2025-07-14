using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(20)]
        public string StudentNumber { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string? Gender { get; set; }
        
        public DateTime? BirthDate { get; set; }
        
        [StringLength(50)]
        public string? Class { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        
        // 导航属性
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
} 