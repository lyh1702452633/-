# 学生信息管理系统

一个基于ASP.NET Core和MySQL的学生信息管理系统，支持学生、教师、管理员三种角色。

## 功能特性

### 系统架构
- 后端：ASP.NET Core 6.0
- 数据库：MySQL 8.0
- 前端：Bootstrap 5 + jQuery
- ORM：Entity Framework Core

### 角色功能

#### 学生角色
- 查看个人信息和统计数据（平均成绩、不及格科目数、出勤率、缺勤次数）
- 查看成绩记录（包含成绩等级和通过状态）
- 查看考勤记录
- 编辑个人资料（邮箱、电话、地址）

#### 教师角色
- 查看教学统计（课程数、学生数、不及格人数、缺勤人数）
- 管理所授课程
- 查看课程学生列表
- 管理学生成绩（支持触发器约束）
- 查看学生考勤记录

#### 管理员角色
- 系统总体统计
- 学生管理（增删查改）
- 教师管理（增删查改）
- 课程管理（增删查改）
- 成绩管理
- 考勤管理

### 数据库特性
- **外键约束**：确保数据完整性
- **视图**：StudentGradeView（学生成绩统计）、StudentAttendanceView（学生考勤统计）
- **触发器**：
  - 防止60分以下成绩修改为60分以上
  - 防止删除有成绩记录的学生
  - 防止删除有课程的教师

## 安装和运行

### 环境要求
- .NET 6.0 SDK
- MySQL 8.0+
- Visual Studio 2022 或 VS Code

### 安装步骤

1. **克隆项目**
   ```bash
   git clone <项目地址>
   cd StudentManagementSystem
   ```

2. **配置数据库**
   - 修改 `appsettings.json` 中的数据库连接字符串
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=StudentManagementSystem;Uid=root;Pwd=your_password;"
     }
   }
   ```

3. **创建数据库**
   - 在MySQL中执行 `SQL/InitDatabase.sql` 脚本

4. **安装依赖包**
   ```bash
   dotnet restore
   ```

5. **运行项目**
   ```bash
   dotnet run
   ```

6. **访问系统**
   - 打开浏览器访问：`https://localhost:5001` 或 `http://localhost:5000`

### 默认账户

| 角色   | 用户名    | 密码        |
|--------|-----------|-------------|
| 管理员 | admin     | admin123    |
| 教师   | teacher1  | teacher123  |
| 学生   | student1  | student123  |
| 学生   | student2  | student123  |

## 数据库设计

### 主要数据表
- `Users` - 用户账户表
- `Students` - 学生信息表
- `Teachers` - 教师信息表
- `Courses` - 课程表
- `Grades` - 成绩表
- `Attendances` - 考勤表

### 视图
- `StudentGradeView` - 学生成绩统计视图
- `StudentAttendanceView` - 学生考勤统计视图

### 触发器
- `trg_grade_update_check` - 成绩修改检查触发器
- `trg_student_delete_check` - 学生删除检查触发器
- `trg_teacher_delete_check` - 教师删除检查触发器

## 项目结构

```
StudentManagementSystem/
├── Controllers/           # 控制器
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── BaseController.cs
│   ├── StudentController.cs
│   └── TeacherController.cs
├── Data/                 # 数据访问层
│   └── ApplicationDbContext.cs
├── Models/               # 数据模型
│   ├── Attendance.cs
│   ├── Course.cs
│   ├── Grade.cs
│   ├── Student.cs
│   ├── Teacher.cs
│   └── User.cs
├── Views/                # 视图文件
│   ├── Account/
│   ├── Admin/
│   ├── Shared/
│   ├── Student/
│   └── Teacher/
├── wwwroot/              # 静态文件
├── SQL/                  # 数据库脚本
│   └── InitDatabase.sql
├── Program.cs            # 程序入口
└── appsettings.json      # 配置文件
```

## 技术特点

1. **MVC架构模式**：清晰的代码结构和职责分离
2. **Entity Framework Core**：代码优先的ORM框架
3. **Session认证**：简单有效的身份验证
4. **Bootstrap响应式设计**：现代化的用户界面
5. **异常处理**：捕获并展示数据库触发器异常
6. **数据验证**：前后端数据验证
7. **权限控制**：基于角色的访问控制

## 开发说明

### 触发器异常处理示例
```csharp
try
{
    grade.Score = newScore;
    await _context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
    if (ex.InnerException?.Message.Contains("不能将60分以下的成绩修改为60分或以上") == true)
    {
        return Json(new { success = false, message = "不能将60分以下的成绩修改为60分或以上！" });
    }
}
```

### 添加新功能
1. 在相应的Controller中添加Action方法
2. 创建对应的View文件
3. 更新导航菜单（在_Layout.cshtml中）
4. 如需要，更新数据模型和数据库

## 许可证

MIT License

## 作者

数据库课程设计项目 