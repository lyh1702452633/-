-- 创建数据库
CREATE DATABASE IF NOT EXISTS StudentManagementSystem CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE StudentManagementSystem;

-- 创建用户表
CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Password VARCHAR(100) NOT NULL,
    Role INT NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE
);

-- 创建学生表
CREATE TABLE Students (
    StudentId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    StudentNumber VARCHAR(20) NOT NULL UNIQUE,
    Name VARCHAR(50) NOT NULL,
    Gender VARCHAR(10),
    BirthDate DATE,
    Class VARCHAR(50),
    Email VARCHAR(100),
    Phone VARCHAR(20),
    Address VARCHAR(200),
    EnrollmentDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- 创建教师表
CREATE TABLE Teachers (
    TeacherId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    TeacherNumber VARCHAR(20) NOT NULL UNIQUE,
    Name VARCHAR(50) NOT NULL,
    Gender VARCHAR(10),
    BirthDate DATE,
    Department VARCHAR(50),
    Title VARCHAR(50),
    Email VARCHAR(100),
    Phone VARCHAR(20),
    HireDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- 创建课程表
CREATE TABLE Courses (
    CourseId INT AUTO_INCREMENT PRIMARY KEY,
    CourseCode VARCHAR(20) NOT NULL UNIQUE,
    CourseName VARCHAR(100) NOT NULL,
    Credits INT NOT NULL,
    TeacherId INT NOT NULL,
    Description VARCHAR(200),
    FOREIGN KEY (TeacherId) REFERENCES Teachers(TeacherId) ON DELETE RESTRICT
);

-- 创建成绩表
CREATE TABLE Grades (
    GradeId INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    Score DECIMAL(5,2) NOT NULL CHECK (Score >= 0 AND Score <= 100),
    ExamDate DATE NOT NULL,
    ExamType VARCHAR(20) NOT NULL,
    Remarks VARCHAR(200),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE
);

-- 创建考勤表
CREATE TABLE Attendances (
    AttendanceId INT AUTO_INCREMENT PRIMARY KEY,
    StudentId INT NOT NULL,
    CourseId INT NOT NULL,
    AttendanceDate DATE NOT NULL,
    Status INT NOT NULL,
    Remarks VARCHAR(200),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId) ON DELETE CASCADE
);

-- 创建学生成绩统计视图
CREATE VIEW StudentGradeView AS
SELECT 
    s.StudentId,
    s.Name AS StudentName,
    s.StudentNumber,
    COUNT(CASE WHEN g.Score < 60 THEN 1 END) AS FailedSubjectCount,
    AVG(g.Score) AS AverageScore
FROM Students s
LEFT JOIN Grades g ON s.StudentId = g.StudentId
GROUP BY s.StudentId, s.Name, s.StudentNumber;

-- 创建学生考勤统计视图
CREATE VIEW StudentAttendanceView AS
SELECT 
    s.StudentId,
    s.Name AS StudentName,
    s.StudentNumber,
    COUNT(a.AttendanceId) AS TotalClasses,
    COUNT(CASE WHEN a.Status = 2 THEN 1 END) AS AbsentCount,
    CASE 
        WHEN COUNT(a.AttendanceId) > 0 THEN 
            ROUND((COUNT(a.AttendanceId) - COUNT(CASE WHEN a.Status = 2 THEN 1 END)) * 100.0 / COUNT(a.AttendanceId), 2)
        ELSE 0 
    END AS AttendanceRate
FROM Students s
LEFT JOIN Attendances a ON s.StudentId = a.StudentId
GROUP BY s.StudentId, s.Name, s.StudentNumber;

-- 创建触发器：防止60分以下成绩修改为60分以上
DELIMITER //
CREATE TRIGGER trg_grade_update_check
    BEFORE UPDATE ON Grades
    FOR EACH ROW
BEGIN
    -- 如果原成绩小于60分，且新成绩大于等于60分，则抛出异常
    IF OLD.Score < 60 AND NEW.Score >= 60 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = '不能将60分以下的成绩修改为60分或以上！';
    END IF;
END//
DELIMITER ;

-- 创建触发器：防止删除有成绩记录的学生
DELIMITER //
CREATE TRIGGER trg_student_delete_check
    BEFORE DELETE ON Students
    FOR EACH ROW
BEGIN
    DECLARE grade_count INT;
    SELECT COUNT(*) INTO grade_count FROM Grades WHERE StudentId = OLD.StudentId;
    
    IF grade_count > 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = '不能删除有成绩记录的学生！';
    END IF;
END//
DELIMITER ;

-- 创建触发器：防止删除有课程的教师
DELIMITER //
CREATE TRIGGER trg_teacher_delete_check
    BEFORE DELETE ON Teachers
    FOR EACH ROW
BEGIN
    DECLARE course_count INT;
    SELECT COUNT(*) INTO course_count FROM Courses WHERE TeacherId = OLD.TeacherId;
    
    IF course_count > 0 THEN
        SIGNAL SQLSTATE '45000' 
        SET MESSAGE_TEXT = '不能删除正在授课的教师！';
    END IF;
END//
DELIMITER ;

-- 插入初始数据
INSERT INTO Users (Username, Password, Role) VALUES 
('admin', 'admin123', 3),
('teacher1', 'teacher123', 2),
('student1', 'student123', 1),
('student2', 'student123', 1);

INSERT INTO Teachers (UserId, TeacherNumber, Name, Gender, Department, Title, Email, Phone) VALUES 
(2, 'T001', '张老师', '男', '计算机科学系', '教授', 'zhang@school.edu', '13800138001');

INSERT INTO Students (UserId, StudentNumber, Name, Gender, Class, Email, Phone) VALUES 
(3, 'S2021001', '李明', '男', '计算机1班', 'liming@student.edu', '13800138002'),
(4, 'S2021002', '王丽', '女', '计算机1班', 'wangli@student.edu', '13800138003');

INSERT INTO Courses (CourseCode, CourseName, Credits, TeacherId, Description) VALUES 
('CS101', '数据结构', 3, 1, '计算机基础课程'),
('CS102', '数据库原理', 3, 1, '数据库系统设计与实现'),
('CS103', 'Java程序设计', 4, 1, '面向对象编程语言');

INSERT INTO Grades (StudentId, CourseId, Score, ExamDate, ExamType) VALUES 
(1, 1, 85, '2023-06-15', '期末考试'),
(1, 2, 55, '2023-06-16', '期末考试'),
(2, 1, 78, '2023-06-15', '期末考试'),
(2, 2, 88, '2023-06-16', '期末考试');

INSERT INTO Attendances (StudentId, CourseId, AttendanceDate, Status) VALUES 
(1, 1, '2023-09-01', 1),
(1, 1, '2023-09-08', 1),
(1, 1, '2023-09-15', 2),
(2, 1, '2023-09-01', 1),
(2, 1, '2023-09-08', 1),
(2, 1, '2023-09-15', 1); 