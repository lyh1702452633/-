using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class TeacherController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!CheckRole(Models.UserRole.Teacher))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == UserId);

            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 获取统计信息
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.TeacherId)
                .ToListAsync();

            var totalStudents = 0;
            var failedStudents = 0;
            var absentStudents = 0;

            foreach (var course in courses)
            {
                var courseStudents = await _context.Grades
                    .Where(g => g.CourseId == course.CourseId)
                    .Select(g => g.StudentId)
                    .Distinct()
                    .CountAsync();

                var courseFailed = await _context.Grades
                    .Where(g => g.CourseId == course.CourseId && g.Score < 60)
                    .Select(g => g.StudentId)
                    .Distinct()
                    .CountAsync();

                var courseAbsent = await _context.Attendances
                    .Where(a => a.CourseId == course.CourseId && a.Status == AttendanceStatus.Absent)
                    .Select(a => a.StudentId)
                    .Distinct()
                    .CountAsync();

                totalStudents += courseStudents;
                failedStudents += courseFailed;
                absentStudents += courseAbsent;
            }

            ViewBag.TotalCourses = courses.Count;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.FailedStudents = failedStudents;
            ViewBag.AbsentStudents = absentStudents;

            return View(teacher);
        }

        public async Task<IActionResult> Courses()
        {
            if (!CheckRole(Models.UserRole.Teacher))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == UserId);

            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.TeacherId)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> Students(int courseId)
        {
            if (!CheckRole(Models.UserRole.Teacher))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == UserId);

            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacher.TeacherId);

            if (course == null)
            {
                return RedirectToAction("Courses");
            }

            var students = await _context.Grades
                .Include(g => g.Student)
                .Where(g => g.CourseId == courseId)
                .Select(g => g.Student)
                .Distinct()
                .ToListAsync();

            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = courseId;
            return View(students);
        }

        public async Task<IActionResult> Grades(int courseId)
        {
            if (!CheckRole(Models.UserRole.Teacher))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == UserId);

            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacher.TeacherId);

            if (course == null)
            {
                return RedirectToAction("Courses");
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .Where(g => g.CourseId == courseId)
                .OrderBy(g => g.Student.StudentNumber)
                .ToListAsync();

            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = courseId;
            return View(grades);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int gradeId, decimal score)
        {
            if (!CheckRole(Models.UserRole.Teacher))
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var grade = await _context.Grades
                    .Include(g => g.Course)
                    .FirstOrDefaultAsync(g => g.GradeId == gradeId);

                if (grade == null)
                {
                    return Json(new { success = false, message = "成绩记录不存在" });
                }

                // 验证教师权限
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.UserId == UserId);

                if (teacher == null || grade.Course.TeacherId != teacher.TeacherId)
                {
                    return Json(new { success = false, message = "无权限修改此成绩" });
                }

                grade.Score = score;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "成绩更新成功" });
            }
            catch (DbUpdateException ex)
            {
                // 捕获触发器异常
                if (ex.InnerException?.Message.Contains("不能将60分以下的成绩修改为60分或以上") == true)
                {
                    return Json(new { success = false, message = "不能将60分以下的成绩修改为60分或以上！" });
                }
                return Json(new { success = false, message = "更新失败：" + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "更新失败：" + ex.Message });
            }
        }

        public async Task<IActionResult> Attendance(int courseId)
        {
            if (!CheckRole(Models.UserRole.Teacher))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == UserId);

            if (teacher == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.TeacherId == teacher.TeacherId);

            if (course == null)
            {
                return RedirectToAction("Courses");
            }

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.AttendanceDate)
                .ThenBy(a => a.Student.StudentNumber)
                .ToListAsync();

            ViewBag.CourseName = course.CourseName;
            ViewBag.CourseId = courseId;
            return View(attendances);
        }
    }
} 