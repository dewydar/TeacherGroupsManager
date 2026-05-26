# Teacher Groups Management System

Teacher Groups Management System is an Arabic RTL web application built with .NET 8 MVC and SQL Server.

The system helps teachers manage academic years, groups, students, lessons, private lessons, group lessons, monthly payments, employees, roles, permissions, reports, and dashboard statistics.

## وصف النظام

نظام إدارة مجموعات المدرس هو نظام عربي بالكامل يساعد المدرس أو السنتر التعليمي على إدارة الطلاب، المجموعات، الدروس، المدفوعات الشهرية، الموظفين، الأدوار، الصلاحيات، والتقارير من خلال لوحة تحكم سهلة وواضحة.

النظام يدعم نوعين من الدروس:

- درس خاص
- درس للمجموعة بالكامل

كما يدعم متابعة المدفوعات الشهرية ومعرفة الطلاب الذين دفعوا والطلاب الذين لم يدفعوا داخل كل مجموعة.

## Main Features

- Arabic RTL user interface
- Login and logout
- Roles and permissions management
- Employees management
- Academic years management
- Groups management
- Students management
- Private and group lessons
- Monthly payment tracking
- Paid, partially paid, and unpaid student status
- Clickable dashboard statistics
- Reports for students, groups, lessons, and payments
- SQL Server database
- Entity Framework Core 8
- Layered architecture
- Services layer
- Unit tests for services

## المميزات الرئيسية

- واجهة عربية من اليمين إلى اليسار
- تسجيل الدخول والخروج
- إدارة الأدوار والصلاحيات
- إدارة الموظفين
- إدارة السنوات الدراسية
- إدارة المجموعات
- إدارة الطلاب
- إدارة الدروس الخاصة ودروس المجموعات
- متابعة المدفوعات الشهرية
- معرفة حالة الدفع: مدفوع، مدفوع جزئي، غير مدفوع
- لوحة تحكم تفاعلية وقابلة للضغط
- تقارير للطلاب والمجموعات والدروس والمدفوعات
- قاعدة بيانات SQL Server
- استخدام Entity Framework Core
- تقسيم المشروع إلى Layers واضحة
- Services Layer
- Unit Tests للـ Services

## Architecture

The solution uses a layered architecture with the following projects:

- TeacherGroupsManager.Core
- TeacherGroupsManager.Data
- TeacherGroupsManager.Shared
- TeacherGroupsManager.Dtos
- TeacherGroupsManager.Services
- TeacherGroupsManager.Services.Tests
- TeacherGroupsManager.WebUI

## Layers Description

### TeacherGroupsManager.Core

Contains domain entities, enums, constants, and base classes.

### TeacherGroupsManager.Data

Contains EF Core DbContext, database models, repositories, Unit of Work, and data access configuration.

### TeacherGroupsManager.Shared

Contains shared helpers, operation results, pagination models, constants, and common extensions.

### TeacherGroupsManager.Dtos

Contains DTOs, request models, response models, and ViewModels.

### TeacherGroupsManager.Services

Contains business services, service interfaces, Mapperly mappers, FluentValidation validators, permissions logic, and dashboard calculations.

### TeacherGroupsManager.Services.Tests

Contains xUnit tests for the services layer.

### TeacherGroupsManager.WebUI

ASP.NET Core MVC Web UI project with Arabic RTL views, controllers, layout, dashboard, menus, and authentication pages.

## Main Modules

- Dashboard
- Roles
- Permissions
- Employees
- Academic Years
- Groups
- Students
- Lessons
- Monthly Payments
- Reports

## Arabic Menu

- لوحة التحكم
- إدارة الأدوار
- إدارة الموظفين
- السنوات الدراسية
- المجموعات
- الطلاب
- الدروس
- المدفوعات الشهرية
- التقارير
- تسجيل الخروج

## Roles

The system supports the following roles:

- Admin - أدمن
- Teacher - مدرس
- AssistantTeacher - مساعد مدرس

## Permissions

- Admin can manage everything.
- Teacher can manage groups, students, lessons, payments, academic years, and assistant teachers.
- AssistantTeacher permissions are configurable.
- Only Admin and Teacher can create employees.

## Business Flow

```text
Role
  ↓
Employee
  ↓
Teacher / Assistant Teacher

Academic Year
  ↓
Groups
  ↓
Students

Groups
  ↓
Lessons
  ↓
Lesson Students

Students
  ↓
Monthly Payments
  ↓
Paid / Partially Paid / Unpaid

Dashboard
  ↓
Reads statistics from:
Students
Groups
Lessons
MonthlyPayments
Employees
AcademicYears
```
