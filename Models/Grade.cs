using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }
        
        [ForeignKey("Student")]
        public int StudentId { get; set; }
        
        [ForeignKey("Course")]
        public int CourseId { get; set; }
        
        [Range(0, 100)]
        public decimal Score { get; set; }
        
        public DateTime ExamDate { get; set; }
        
        [StringLength(20)]
        public string ExamType { get; set; } = "其他"; // 期中、期末、平时
        
        [StringLength(200)]
        public string? Remarks { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // 导航属性
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
} 