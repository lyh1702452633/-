using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class StudentController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!CheckRole(Models.UserRole.Student))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 获取统计信息
            var gradeStats = await _context.StudentGradeView
                .FirstOrDefaultAsync(v => v.StudentId == student.StudentId);

            var attendanceStats = await _context.StudentAttendanceView
                .FirstOrDefaultAsync(v => v.StudentId == student.StudentId);

            ViewBag.FailedSubjectCount = gradeStats?.FailedSubjectCount ?? 0;
            ViewBag.AverageScore = gradeStats?.AverageScore ?? 0;
            ViewBag.AttendanceRate = attendanceStats?.AttendanceRate ?? 0;
            ViewBag.AbsentCount = attendanceStats?.AbsentCount ?? 0;

            return View(student);
        }

        public async Task<IActionResult> Grades()
        {
            if (!CheckRole(Models.UserRole.Student))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var grades = await _context.Grades
                .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
                .Where(g => g.StudentId == student.StudentId)
                .OrderByDescending(g => g.ExamDate)
                .ToListAsync();

            ViewBag.StudentName = student.Name;
            return View(grades);
        }

        public async Task<IActionResult> Attendance()
        {
            if (!CheckRole(Models.UserRole.Student))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var attendances = await _context.Attendances
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .Where(a => a.StudentId == student.StudentId)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            ViewBag.StudentName = student.Name;
            return View(attendances);
        }

        public async Task<IActionResult> Profile()
        {
            if (!CheckRole(Models.UserRole.Student))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Student model)
        {
            if (!CheckRole(Models.UserRole.Student))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == UserId);

            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 只允许修改部分信息
            student.Email = model.Email;
            student.Phone = model.Phone;
            student.Address = model.Address;

            try
            {
                await _context.SaveChangesAsync();
                ViewBag.Success = "个人信息更新成功！";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "更新失败：" + ex.Message;
            }

            return View(student);
        }
    }
} 