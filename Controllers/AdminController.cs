using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentManagementSystem.Data;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 数据库连接测试
        public async Task<IActionResult> TestConnection()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 测试数据库连接
                await _context.Database.CanConnectAsync();
                
                // 测试简单查询
                var userCount = await _context.Users.CountAsync();
                
                ViewBag.Message = $"数据库连接正常，当前用户数量：{userCount}";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"数据库连接失败：{ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> Index()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // 获取统计信息
            var totalStudents = await _context.Students.CountAsync();
            var totalTeachers = await _context.Teachers.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalTeachers = totalTeachers;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalUsers = totalUsers;

            return View();
        }

        // 学生管理
        public async Task<IActionResult> Students()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var students = await _context.Students
                    .Include(s => s.User)
                    .OrderBy(s => s.StudentNumber)
                    .ToListAsync();

                return View(students);
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"获取学生列表时发生错误：{ex.Message}");
                
                // 返回错误信息给用户
                ViewBag.Error = "获取学生信息时发生错误，请检查数据库连接或联系管理员。";
                return View(new List<Student>());
            }
        }

        public IActionResult CreateStudent()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(Student student, string username, string password)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 数据验证
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "用户名和密码不能为空！";
                    return View(student);
                }

                // 检查用户名是否已存在
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (existingUser != null)
                {
                    ViewBag.Error = "用户名已存在！";
                    return View(student);
                }

                // 检查学号是否已存在
                var existingStudent = await _context.Students.FirstOrDefaultAsync(s => s.StudentNumber == student.StudentNumber);
                if (existingStudent != null)
                {
                    ViewBag.Error = "学号已存在！";
                    return View(student);
                }

                // 清理空字符串为null
                student.Gender = string.IsNullOrWhiteSpace(student.Gender) ? null : student.Gender.Trim();
                student.Class = string.IsNullOrWhiteSpace(student.Class) ? null : student.Class.Trim();
                student.Email = string.IsNullOrWhiteSpace(student.Email) ? null : student.Email.Trim();
                student.Phone = string.IsNullOrWhiteSpace(student.Phone) ? null : student.Phone.Trim();
                student.Address = string.IsNullOrWhiteSpace(student.Address) ? null : student.Address.Trim();

                // 创建用户账户
                var user = new User
                {
                    Username = username.Trim(),
                    Password = password,
                    Role = Models.UserRole.Student
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 创建学生信息
                student.UserId = user.UserId;
                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                return RedirectToAction("Students");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "创建失败：" + ex.Message;
                return View(student);
            }
        }

        public async Task<IActionResult> EditStudent(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(Student student)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 检查学号是否已被其他学生使用
                var existingStudent = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentNumber == student.StudentNumber && s.StudentId != student.StudentId);
                if (existingStudent != null)
                {
                    ViewBag.Error = "学号已被其他学生使用！";
                    
                    // 重新加载学生信息以保持导航属性
                    var studentWithUser = await _context.Students
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.StudentId == student.StudentId);
                    
                    return View(studentWithUser);
                }

                // 清理空字符串为null
                student.Gender = string.IsNullOrWhiteSpace(student.Gender) ? null : student.Gender.Trim();
                student.Class = string.IsNullOrWhiteSpace(student.Class) ? null : student.Class.Trim();
                student.Email = string.IsNullOrWhiteSpace(student.Email) ? null : student.Email.Trim();
                student.Phone = string.IsNullOrWhiteSpace(student.Phone) ? null : student.Phone.Trim();
                student.Address = string.IsNullOrWhiteSpace(student.Address) ? null : student.Address.Trim();

                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction("Students");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "更新失败：" + ex.Message;
                
                // 重新加载学生信息以保持导航属性
                var studentWithUser = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == student.StudentId);
                
                return View(studentWithUser);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return Json(new { success = false, message = "权限不足" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (student == null)
                {
                    return Json(new { success = false, message = "学生不存在" });
                }

                // 检查是否有相关记录
                var hasGrades = await _context.Grades.AnyAsync(g => g.StudentId == id);
                var hasAttendances = await _context.Attendances.AnyAsync(a => a.StudentId == id);

                if (hasGrades || hasAttendances)
                {
                    // 提示用户确认删除相关数据
                    var gradeCount = await _context.Grades.CountAsync(g => g.StudentId == id);
                    var attendanceCount = await _context.Attendances.CountAsync(a => a.StudentId == id);
                    
                    var message = $"该学生有 {gradeCount} 条成绩记录和 {attendanceCount} 条考勤记录。";
                    
                    // 先删除所有相关的成绩记录
                    var grades = await _context.Grades.Where(g => g.StudentId == id).ToListAsync();
                    _context.Grades.RemoveRange(grades);
                    
                    // 删除所有相关的考勤记录
                    var attendances = await _context.Attendances.Where(a => a.StudentId == id).ToListAsync();
                    _context.Attendances.RemoveRange(attendances);
                    
                    await _context.SaveChangesAsync();
                }

                // 现在可以安全删除学生和用户记录
                _context.Students.Remove(student);
                _context.Users.Remove(student.User);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = "删除成功" });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                // 捕获触发器异常
                if (ex.InnerException?.Message.Contains("不能删除有成绩记录的学生") == true)
                {
                    return Json(new { success = false, message = "删除失败：存在关联数据，请联系系统管理员！" });
                }
                return Json(new { success = false, message = "删除失败：" + ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "删除失败：" + ex.Message });
            }
        }

        // 教师管理
        public async Task<IActionResult> Teachers()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var teachers = await _context.Teachers
                    .Include(t => t.User)
                    .OrderBy(t => t.TeacherNumber)
                    .ToListAsync();

                return View(teachers);
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"获取教师列表时发生错误：{ex.Message}");
                
                // 返回错误信息给用户
                ViewBag.Error = "获取教师信息时发生错误，请检查数据库连接或联系管理员。";
                return View(new List<Teacher>());
            }
        }

        public IActionResult CreateTeacher()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(Teacher teacher, string username, string password)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 数据验证
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "用户名和密码不能为空！";
                    return View(teacher);
                }

                // 检查用户名是否已存在
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (existingUser != null)
                {
                    ViewBag.Error = "用户名已存在！";
                    return View(teacher);
                }

                // 检查教师编号是否已存在
                var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherNumber == teacher.TeacherNumber);
                if (existingTeacher != null)
                {
                    ViewBag.Error = "教师编号已存在！";
                    return View(teacher);
                }

                // 清理空字符串为null
                teacher.Gender = string.IsNullOrWhiteSpace(teacher.Gender) ? null : teacher.Gender.Trim();
                teacher.Department = string.IsNullOrWhiteSpace(teacher.Department) ? null : teacher.Department.Trim();
                teacher.Title = string.IsNullOrWhiteSpace(teacher.Title) ? null : teacher.Title.Trim();
                teacher.Email = string.IsNullOrWhiteSpace(teacher.Email) ? null : teacher.Email.Trim();
                teacher.Phone = string.IsNullOrWhiteSpace(teacher.Phone) ? null : teacher.Phone.Trim();

                // 创建用户账户
                var user = new User
                {
                    Username = username.Trim(),
                    Password = password,
                    Role = Models.UserRole.Teacher
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 创建教师信息
                teacher.UserId = user.UserId;
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                return RedirectToAction("Teachers");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "创建失败：" + ex.Message;
                return View(teacher);
            }
        }

        public async Task<IActionResult> EditTeacher(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> EditTeacher(Teacher teacher)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 检查教师编号是否已被其他教师使用
                var existingTeacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.TeacherNumber == teacher.TeacherNumber && t.TeacherId != teacher.TeacherId);
                if (existingTeacher != null)
                {
                    ViewBag.Error = "教师编号已被其他教师使用！";
                    
                    // 重新加载教师信息以保持导航属性
                    var teacherWithUser = await _context.Teachers
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.TeacherId == teacher.TeacherId);
                    
                    return View(teacherWithUser);
                }

                // 清理空字符串为null
                teacher.Gender = string.IsNullOrWhiteSpace(teacher.Gender) ? null : teacher.Gender.Trim();
                teacher.Department = string.IsNullOrWhiteSpace(teacher.Department) ? null : teacher.Department.Trim();
                teacher.Title = string.IsNullOrWhiteSpace(teacher.Title) ? null : teacher.Title.Trim();
                teacher.Email = string.IsNullOrWhiteSpace(teacher.Email) ? null : teacher.Email.Trim();
                teacher.Phone = string.IsNullOrWhiteSpace(teacher.Phone) ? null : teacher.Phone.Trim();

                _context.Teachers.Update(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction("Teachers");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "更新失败：" + ex.Message;
                
                // 重新加载教师信息以保持导航属性
                var teacherWithUser = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == teacher.TeacherId);
                
                return View(teacherWithUser);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return Json(new { success = false, message = "权限不足" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var teacher = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    return Json(new { success = false, message = "教师不存在" });
                }

                // 检查是否有关联的课程
                var courses = await _context.Courses.Where(c => c.TeacherId == id).ToListAsync();
                if (courses.Any())
                {
                    // 统计相关数据
                    int totalGrades = 0;
                    int totalAttendances = 0;
                    
                    foreach (var course in courses)
                    {
                        totalGrades += await _context.Grades.CountAsync(g => g.CourseId == course.CourseId);
                        totalAttendances += await _context.Attendances.CountAsync(a => a.CourseId == course.CourseId);
                    }

                    // 删除所有相关数据
                    foreach (var course in courses)
                    {
                        // 删除课程的成绩记录
                        var grades = await _context.Grades.Where(g => g.CourseId == course.CourseId).ToListAsync();
                        _context.Grades.RemoveRange(grades);
                        
                        // 删除课程的考勤记录
                        var attendances = await _context.Attendances.Where(a => a.CourseId == course.CourseId).ToListAsync();
                        _context.Attendances.RemoveRange(attendances);
                    }
                    
                    // 删除课程
                    _context.Courses.RemoveRange(courses);
                    
                    await _context.SaveChangesAsync();
                }

                // 现在可以安全删除教师和用户记录
                _context.Teachers.Remove(teacher);
                _context.Users.Remove(teacher.User);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Json(new { success = true, message = "删除成功" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "删除失败：" + ex.Message });
            }
        }

        // 课程管理
        public async Task<IActionResult> Courses()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> CreateCourse()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Courses");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "创建失败：" + ex.Message;
                ViewBag.Teachers = await _context.Teachers.ToListAsync();
                return View(course);
            }
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Teachers = await _context.Teachers.ToListAsync();
            return View(course);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(Course course)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 检查课程代码是否已被其他课程使用
                var existingCourse = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseCode == course.CourseCode && c.CourseId != course.CourseId);
                if (existingCourse != null)
                {
                    ViewBag.Error = "课程代码已被其他课程使用！";
                    ViewBag.Teachers = await _context.Teachers.ToListAsync();
                    return View(course);
                }

                // 清理空字符串为null
                course.Description = string.IsNullOrWhiteSpace(course.Description) ? null : course.Description.Trim();

                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction("Courses");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "更新失败：" + ex.Message;
                ViewBag.Teachers = await _context.Teachers.ToListAsync();
                return View(course);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == id);

                if (course == null)
                {
                    return Json(new { success = false, message = "课程不存在" });
                }

                // 检查是否有关联的成绩记录
                var hasGrades = await _context.Grades.AnyAsync(g => g.CourseId == id);
                if (hasGrades)
                {
                    return Json(new { success = false, message = "不能删除有成绩记录的课程！" });
                }

                // 检查是否有关联的考勤记录
                var hasAttendances = await _context.Attendances.AnyAsync(a => a.CourseId == id);
                if (hasAttendances)
                {
                    return Json(new { success = false, message = "不能删除有考勤记录的课程！" });
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "删除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "删除失败：" + ex.Message });
            }
        }

        // 成绩管理
        public async Task<IActionResult> Grades()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return View(grades);
        }

        public async Task<IActionResult> CreateGrade()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
            ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGrade(Grade grade)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 数据验证
                if (grade.StudentId <= 0 || grade.CourseId <= 0)
                {
                    ViewBag.Error = "请选择学生和课程！";
                    ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                    ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                    return View(grade);
                }

                // 检查学生是否存在
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == grade.StudentId);
                if (student == null)
                {
                    ViewBag.Error = "选择的学生不存在！";
                    ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                    ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                    return View(grade);
                }

                // 检查课程是否存在
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == grade.CourseId);
                if (course == null)
                {
                    ViewBag.Error = "选择的课程不存在！";
                    ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                    ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                    return View(grade);
                }

                // 检查是否已存在该学生该课程的相同类型成绩
                var existingGrade = await _context.Grades
                    .FirstOrDefaultAsync(g => g.StudentId == grade.StudentId && 
                                             g.CourseId == grade.CourseId && 
                                             g.ExamType == grade.ExamType);
                if (existingGrade != null)
                {
                    ViewBag.Error = $"该学生在此课程中已存在相同类型({grade.ExamType})的成绩记录！";
                    ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                    ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                    return View(grade);
                }

                // 清理空字符串，ExamType不能为null，设置默认值
                grade.ExamType = string.IsNullOrWhiteSpace(grade.ExamType) ? "其他" : grade.ExamType.Trim();
                grade.Remarks = string.IsNullOrWhiteSpace(grade.Remarks) ? null : grade.Remarks.Trim();

                _context.Grades.Add(grade);
                await _context.SaveChangesAsync();

                return RedirectToAction("Grades");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "创建失败：" + ex.Message;
                ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                return View(grade);
            }
        }

        public async Task<IActionResult> EditGrade(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Course)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(g => g.GradeId == id);

            if (grade == null)
            {
                return NotFound();
            }

            ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
            ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
            return View(grade);
        }

        [HttpPost]
        public async Task<IActionResult> EditGrade(Grade grade)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                // 检查成绩记录是否存在（使用AsNoTracking避免跟踪冲突）
                var existingGrade = await _context.Grades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.GradeId == grade.GradeId);
                if (existingGrade == null)
                {
                    ViewBag.Error = "成绩记录不存在！";
                    ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                    ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                    return View(grade);
                }

                // 检查是否已存在该学生该课程的相同类型成绩（排除当前记录）
                var duplicateGrade = await _context.Grades
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.StudentId == grade.StudentId && 
                                             g.CourseId == grade.CourseId && 
                                             g.ExamType == grade.ExamType &&
                                             g.GradeId != grade.GradeId);
                if (duplicateGrade != null)
                {
                    ViewBag.Error = $"该学生在此课程中已存在相同类型({grade.ExamType})的成绩记录！";
                    ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                    ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                    return View(grade);
                }

                // 清理空字符串，ExamType不能为null，设置默认值
                grade.ExamType = string.IsNullOrWhiteSpace(grade.ExamType) ? "其他" : grade.ExamType.Trim();
                grade.Remarks = string.IsNullOrWhiteSpace(grade.Remarks) ? null : grade.Remarks.Trim();

                _context.Grades.Update(grade);
                await _context.SaveChangesAsync();
                return RedirectToAction("Grades");
            }
            catch (DbUpdateException ex)
            {
                // 捕获触发器异常
                if (ex.InnerException?.Message.Contains("不能将60分以下的成绩修改为60分或以上") == true)
                {
                    ViewBag.Error = "不能将60分以下的成绩修改为60分或以上！";
                }
                else
                {
                    ViewBag.Error = "更新失败：" + ex.Message;
                }
                
                ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                return View(grade);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "更新失败：" + ex.Message;
                ViewBag.Students = await _context.Students.OrderBy(s => s.StudentNumber).ToListAsync();
                ViewBag.Courses = await _context.Courses.Include(c => c.Teacher).OrderBy(c => c.CourseCode).ToListAsync();
                return View(grade);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.GradeId == id);

                if (grade == null)
                {
                    return Json(new { success = false, message = "成绩记录不存在" });
                }

                _context.Grades.Remove(grade);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "删除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "删除失败：" + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int gradeId, decimal score)
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return Json(new { success = false, message = "权限不足" });
            }

            try
            {
                // 先检查成绩记录是否存在（不跟踪）
                var gradeExists = await _context.Grades
                    .AsNoTracking()
                    .AnyAsync(g => g.GradeId == gradeId);

                if (!gradeExists)
                {
                    return Json(new { success = false, message = "成绩记录不存在" });
                }

                // 使用附加和修改状态的方式更新，避免跟踪冲突
                var grade = new Grade { GradeId = gradeId, Score = score };
                _context.Grades.Attach(grade);
                _context.Entry(grade).Property(g => g.Score).IsModified = true;
                
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

        // 考勤管理
        public async Task<IActionResult> Attendances()
        {
            if (!CheckRole(Models.UserRole.Admin))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .ThenInclude(c => c.Teacher)
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();

            return View(attendances);
        }
    }
} 