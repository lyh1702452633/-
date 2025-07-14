-- 测试数据库连接和数据的SQL脚本
-- 在运行ASP.NET项目前，可以用这个脚本测试数据库是否正常

USE StudentManagementSystem;

-- 查看所有表
SHOW TABLES;

-- 查看用户数据
SELECT * FROM Users;

-- 查看学生数据
SELECT s.*, u.Username 
FROM Students s 
JOIN Users u ON s.UserId = u.UserId;

-- 查看教师数据
SELECT t.*, u.Username 
FROM Teachers t 
JOIN Users u ON t.UserId = u.UserId;

-- 查看课程数据
SELECT c.*, t.Name as TeacherName 
FROM Courses c 
JOIN Teachers t ON c.TeacherId = t.TeacherId;

-- 查看成绩数据
SELECT g.*, s.Name as StudentName, c.CourseName 
FROM Grades g 
JOIN Students s ON g.StudentId = s.StudentId 
JOIN Courses c ON g.CourseId = c.CourseId;

-- 查看考勤数据
SELECT a.*, s.Name as StudentName, c.CourseName,
    CASE a.Status 
        WHEN 1 THEN '出勤'
        WHEN 2 THEN '缺勤' 
        WHEN 3 THEN '迟到'
        WHEN 4 THEN '请假'
    END as StatusText
FROM Attendances a 
JOIN Students s ON a.StudentId = s.StudentId 
JOIN Courses c ON a.CourseId = c.CourseId;

-- 测试视图
SELECT * FROM StudentGradeView;
SELECT * FROM StudentAttendanceView;

-- 测试触发器（这个会失败，用来验证触发器工作）
-- UPDATE Grades SET Score = 65 WHERE Score = 55 LIMIT 1; 