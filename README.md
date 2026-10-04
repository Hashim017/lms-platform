# Lumen LMS

A full learning management system built with ASP.NET Core MVC, Entity Framework Core and PostgreSQL.

**Live demo:** https://lumen-lms.onrender.com/

The free server sleeps when idle. The first visit can take about 30 seconds.

## Features

**Students**
- Browse and search 25 demo courses with live filters
- Enroll in one click
- Open lessons, mark them complete and watch the progress ring fill
- Take quizzes one question at a time, then see a full answer review
- Submit assignments and read grades and feedback
- My learning page with stats

**Instructors**
- Create, edit, publish and delete courses and lessons
- Build quizzes with multiple choice questions
- Create assignments with due dates and grade submissions
- See quiz attempts for each quiz

**Admins**
- Dashboard with platform stats, a 7 day enrollment chart and top courses
- User management with role changes

**Everyone**
- Custom login, sign up and account settings pages
- Dark and light themes
- Animated UI with a responsive layout

## Tech stack

- ASP.NET Core 9 MVC
- ASP.NET Core Identity with roles
- Entity Framework Core with Npgsql
- Neon PostgreSQL
- Bootstrap 5, Bootstrap Icons and vanilla JavaScript
- Docker, deployed on Render

## Run it locally

1. Install the .NET 9 SDK and create a PostgreSQL database.
2. Clone the repo.

```
   git clone https://github.com/Hashim017/lumen-lms.git
   cd lumen-lms
```

3. Set your secrets.

```
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_POSTGRES_CONNECTION_STRING"
   dotnet user-secrets set "SeedDemoData" "true"
```

4. Run the app.

```
   dotnet run
```

The app applies the migrations on the first start. With `SeedDemoData` set to `true`, it also adds 25 courses, 4 instructors, 11 students, quizzes, assignments, progress and grades.

## Demo accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@lms.com | Admin@123 |
| Instructor | instructor@lms.com | Instructor@123 |
| Student | student@lms.com | Student@123 |

More demo users exist, such as `ayesha@lms.com` as an instructor and `usman@lms.com` as a student. Their passwords follow the same pattern as the table.

New sign ups get the Student role. An admin can change roles on the Users page.

## Project structure

```
Controllers/   Account, Admin, Assignments, Courses, Enrollments, Learn, Lessons, Quizzes
Data/          DbContext, seeder, demo content and claims factory
Models/        Entities and view models
Views/         Razor views and shared partials
wwwroot/       CSS and JavaScript
Migrations/    EF Core migrations
```

## Deploy

The app ships with a Dockerfile. On Render, set these environment variables:

- `ConnectionStrings__DefaultConnection` with your PostgreSQL connection string
- `SeedDemoData` with `true` or `false`

The health check path is `/healthz`.

## Author

Built by Muhammad Hashim.