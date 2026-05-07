# 📝 Notes API - Clean Architecture

یک Web API برای مدیریت یادداشت‌ها (Notes) با معماری تمیز (Clean Architecture)، الگوی CQRS و MediatR.

---

## 📁 ساختار پروژه
```markdown
NotePad-CleanArchitecture/
├── 📄 Directory.Build.props ← تنظیمات مشترک Build (TargetFramework, ImplicitUsings,Nullable)
├── 📄 Directory.Packages.props ← مدیریت متمرکز نسخه پکیج‌های NuGet
│ 
├───📦 src
│   ├───⚙️ Application
│   │   ├───Behaviors
│   │   ├───Common
│   │   │   ├───Exceptions
│   │   │   └───Mappings
│   │   └───Controller
│   │       ├───Auth
│   │       │   ├───Commands
│   │       │   └───Queries
│   │       └───Notes
│   │           ├───Commands
│   │           │   ├───CreateNote
│   │           │   ├───DeleteNote
│   │           │   └───UpdateNote
│   │           └───Queries
│   │               ├───GetNoteById
│   │               └───GetNotes
│   ├───🎯 Domain
│   │   ├───Common
│   │   ├───Entities
│   │   ├───Enums
│   │   ├───Events
│   │   ├───Extensions
│   │   ├───Interfaces
│   │   ├───Results
│   │   │   └───Auth
│   │   └───Settings
│   │
│   ├───🏗️ Infrastructure
│   │   ├───Messaging
│   │   │   └───Consumers
│   │   ├───Migrations
│   │   ├───Persistence
│   │   ├───SeedData
│   │   ├───Services
│   │   └───Transformers
│   │
│   ├───📦 Shared
│   │
│   └───🌐 WebApi
│       ├───Controllers
│       └───Properties
│   
└───🧪 tests
    ├───🔄 Application.UnitTests
    │   ├───Behaviors
    │   ├───Notes
    │   │   ├───Commands
    │   │   └───Queries
    │   └───Validators
    ├───🔬 Domain.UnitTests
    │   ├───Entities
    ├───⚡ Infrastructure.IntegrationTests
    │   └───Persistence
    └───🚀 WebApi.FunctionalTests
```


## 🛠️ تکنولوژی‌های اصلی

| تکنولوژی | توضیح |
|----------|-------|
| **.NET 10** | چارچوب اصلی برنامه |
| **C# 13** | زبان برنامه‌نویسی |
| **SQL Server** | دیتابیس اصلی |
| **Redis** | کش توزیع شده |
| **RabbitMQ** | پیام‌رسان (Message Bus) |


---
# 📋 لیست کامل قابلیت‌ها و تکنولوژی‌های اضافه شده به پروژه


## 🏗️ معماری و لایه‌بندی

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **Clean Architecture** | لایه‌بندی Domain, Application, Infrastructure, Presentation (WebApi) با رعایت اصل وابستگی (Dependency Rule) که لایه‌های داخلی به لایه‌های خارجی وابسته نیستند |
| 2 | **CQRS (Command Query Responsibility Segregation)** | جداسازی کامل عملیات Command (نوشتن/تغییر داده) از Query (خواندن داده) با استفاده از MediatR |
| 3 | **MediatR** | پیاده‌سازی الگوی Mediator برای کاهش وابستگی مستقیم بین کلاس‌ها و ارسال درخواست‌ها از طریق Pipeline |
| 4 | **Dependency Injection** | وارونگی کنترل (IoC) با استفاده از Container داخلی ASP.NET Core و ثبت تمام سرویس‌ها در لایه‌های مختلف |
| 5 | **Result Pattern** | استانداردسازی خروجی‌ها با کلاس‌های Result، Error و ApiResponse برای مدیریت یکپارچه خطاها و موفقیت |
| 6 | **Manual Mapping** | نگاشت دستی بین Entity و DTO در Handlers بدون استفاده از AutoMapper (شفاف و قابل debug) |
| 7 | **Unit of Work** | مدیریت تراکنش‌های دیتابیس با استفاده از DbContext و SaveChangesAsync برای اتمی بودن عملیات |
| 8 | **SOLID Principles** | رعایت اصول SOLID: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion |
| 9 | **Pipeline Behavior Pattern** | زنجیره‌ای از Behaviors برای پردازش درخواست‌ها شامل Logging, Validation, Performance, Retry, Caching |
| 10 | **Event-Driven Architecture** | معماری رویداد‌محور با استفاده از RabbitMQ و انتشار رویدادهای دامنه (Domain Events) |
| 11 | **Onion Architecture** | معماری پیازی با Core در مرکز و وابستگی‌های خارجی در لایه‌های بیرونی (Infrastructure و WebApi) |
| 12 | **Strategy Pattern** | استفاده از استراتژی‌های مختلف برای کش (Redis/InMemory) و پیام‌رسان (RabbitMQ/ServiceBus) |
| 13 | **Factory Pattern** | ایجاد اشیاء پیچیده مانند IModel در RabbitMQ و HttpClientFactory برای مدیریت HttpClient |
| 14 | **Singleton Pattern** | مدیریت اتصالات Singleton مانند Redis Connection, RabbitMQ Connection, ILogger |
| 15 | **Options Pattern** | استفاده از IOptions{T} برای دسترسی strongly-typed به تنظیمات appsettings.json در کل پروژه |
| 16 | **Decorator Pattern** | تزئین DbContext با Behaviors در MediatR برای اعتبارسنجی، لاگ و Retry |
| 17 | **Middleware Pipeline** | زنجیره‌ای از Middlewareها برای پردازش درخواست‌های HTTP شامل Authentication, Authorization, Exception Handling |
| 18 | **Background Service Pattern** | پیاده‌سازی مصرف‌کنندگان RabbitMQ به عنوان BackgroundService برای اجرای طولانی‌مدت در پس‌زمینه |
| 19 | **AOP (Aspect-Oriented Programming)** | برنامه‌نویسی جنبه‌گرا با استفاده از Behaviors در MediatR برای جداسازی دغدغه‌های عمومی |
| 20 | **DTO Pattern** | استفاده از Data Transfer Objects برای انتقال داده بین لایه‌ها (Application و WebApi) |
| 21 | **Value Object Pattern** | الگوی Value Object برای اشیاء بدون هویت مانند Email, Password, Address در لایه Domain |

---

## 🔐 امنیت و احراز هویت

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **JWT Authentication** | احراز هویت با توکن JWT |
| 2 | **Refresh Token** | بازخوانی توکن بدون نیاز به لاگین مجدد |
| 3 | **ASP.NET Core Identity** | مدیریت کاربران و نقش‌ها |
| 4 | **Role-Based Authorization** | دسترسی بر اساس نقش (Admin, User) |
| 5 | **Password Hashing** | رمزنگاری رمز عبور |
| 6 | **Account Lockout** | قفل شدن حساب پس از 5 تلاش ناموفق |

---

## 🗄️ دیتابیس

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **SQL Server** | دیتابیس اصلی |
| 2 | **Entity Framework Core** | ORM اصلی برای عملیات نوشتن |
| 3 | **Dapper** | ORM سریع برای عملیات خواندن |
| 4 | **Code First Migration** | مدیریت تغییرات دیتابیس |
| 5 | **Fluent API** | تنظیمات روابط دیتابیس |
| 6 | **InMemory Database** | برای تست‌ها |

---

## 🚀 کش و کارایی

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **Redis Cache** | کش توزیع شده |
| 2 | **IDistributedCache** | اینترفیس استاندارد کش |
| 3 | **Cache Invalidation** | حذف خودکار کش پس از تغییر داده |
| 4 | **Performance Monitoring** | ثبت زمان اجرای درخواست‌ها |

---

## 📨 پیام‌رسان (Message Bus)

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **RabbitMQ** | پیام‌رسان |
| 2 | **Event-Driven Architecture** | انتشار رویدادها |
| 3 | **BackgroundService Consumer** | مصرف‌کننده پس‌زمینه |
| 4 | **Connection Recovery** | اتصال مجدد خودکار |
| 5 | **Retry Policy (Polly)** | تلاش مجدد در صورت خطا |

---

## 📧 ایمیل

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **MailKit** | ارسال ایمیل |
| 2 | **Email Templates** | قالب‌های HTML ایمیل |
| 3 | **SMTP Configuration** | تنظیمات سرور ایمیل |

---

## 📝 اعتبارسنجی

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **FluentValidation** | اعتبارسنجی داده‌ها |
| 2 | **ValidationBehavior** | Pipeline برای اعتبارسنجی خودکار |
| 3 | **Custom Validators** | اعتبارسنجی سفارشی |

---

## 📊 لاگ و مانیتورینگ

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **Serilog** | لاگ ساختاریافته |
| 2 | **LoggingBehavior** | Pipeline برای لاگ خودکار |
| 3 | **File Sink** | ذخیره لاگ در فایل |
| 4 | **Console Sink** | نمایش لاگ در کنسول |
| 5 | **Performance Logging** | ثبت زمان اجرا |
| 6 | **Error Logging** | ثبت خطاها با جزئیات |

---

## 🧪 Pipeline Behaviors (MediatR)

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **LoggingBehavior** | لاگ خودکار درخواست‌ها |
| 2 | **ValidationBehavior** | اعتبارسنجی خودکار |
| 3 | **PerformanceBehavior** | ثبت زمان اجرا |
| 4 | **RetryBehavior** | تلاش مجدد در خطاهای گذرا |

---

## 🌐 API و مستندات

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **RESTful API** | API استاندارد |
| 2 | **API Versioning** | نسخه‌بندی API (v1, v2) |
| 3 | **OpenAPI** | مستندات خودکار |
| 4 | **Scalar UI** | UI تعاملی پیشرفته |

---

## 🧩 Middleware

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **Authentication Middleware** | احراز هویت |
| 2 | **Authorization Middleware** | تعیین دسترسی |
| 3 | **Error Handling Middleware** | مدیریت خطاها |


---

## 🛡️ مدیریت خطا

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **Global Exception Handling** | مدیریت سراسری خطاها |
| 2 | **Custom Exceptions** | استثناهای سفارشی |
| 3 | **DomainException** | خطاهای دامنه |
| 4 | **NotFoundException** | خطای عدم وجود |
| 5 | **UnauthorizedException** | خطای عدم دسترسی |

---

## 🧪 تست

| ردیف | قابلیت | توضیح |
|------|--------|-------|
| 1 | **Unit Tests (xUnit)** | تست واحد |
| 2 | **Moq** | Mock کردن وابستگی‌ها |
| 3 | **FluentAssertions** | بررسی‌های خواناتر |
| 4 | **InMemory Database** | دیتابیس در حافظه برای تست |

---

## 📦 پکیج‌های NuGet اضافه شده

| ردیف | پکیج |
|------|------|
| 1 | MediatR 
| 2 | FluentValidation
| 3 | AutoMapper 
| 4 | Entity Framework Core 
| 5 | Dapper
| 6 | RabbitMQ.Client
| 7 | StackExchange.Redis 
| 8 | MailKit
| 9 | Polly 
| 10 | JwtBearer 
| 11 | Serilog 
| 12 | Scalar.AspNetCore 
| 13 | Asp.Versioning.Mvc 
| 14 | Microsoft.Extensions.Http

---
# 📝 نسخه Package Manager Console (Visual Studio)


```markdown
# 🌱 ایجاد Migration
Add-Migration InitialCreate -Context ApplicationDbContext

# ⬆️ آپدیت دیتابیس
Update-Database -Context ApplicationDbContext

# 🔄 حذف آخرین Migration
Remove-Migration -Context ApplicationDbContext

# ⏪ برگشت به Migration خاص
Update-Database -Migration 20250101000000_InitialCreate -Context ApplicationDbContext

# 📋 لیست Migration‌ها
Get-Migrations -Context ApplicationDbContext

# 💣 حذف دیتابیس
Drop-Database -Context ApplicationDbContext -Force

# 📜 ساخت اسکریپت
Script-Migration -Context ApplicationDbContext

# 🔀 اسکریپت بین دو Migration
Script-Migration -From PreviousMigration -To NextMigration -Context ApplicationDbContext
```
