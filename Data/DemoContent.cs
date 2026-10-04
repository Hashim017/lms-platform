using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data;

public static class DemoContent
{
    private record Seed(
        string Title,
        string Category,
        string Level,
        string Description,
        string[] Lessons,
        (string Q, string Right, string W1, string W2)[] Quiz);

    private static readonly Seed[] Seeds =
    {
        new("Python for Beginners", "Programming", "Beginner",
            "Start coding with Python. Learn variables, lists, loops and functions.",
            new[] { "Install Python and run code", "Variables and data types", "Lists and loops", "Functions and modules" },
            new[] { ("Which function prints text in Python?", "print()", "echo()", "write()"),
                    ("Which symbol starts a comment in Python?", "#", "//", "--") }),

        new("JavaScript Essentials", "Programming", "Beginner",
            "Learn the core of JavaScript and make web pages react to users.",
            new[] { "Values and variables", "Functions and scope", "Arrays and objects", "Working with the DOM" },
            new[] { ("Which keyword declares a block scoped variable?", "let", "var", "dim"),
                    ("Which method adds an item to the end of an array?", "push()", "pop()", "shift()") }),

        new("Java Fundamentals", "Programming", "Beginner",
            "Learn Java from the first program to classes and exceptions.",
            new[] { "Your first Java program", "Types and operators", "Classes and objects", "Exceptions" },
            new[] { ("Which keyword creates a new object in Java?", "new", "create", "make"),
                    ("What is the entry point method in Java?", "main", "start", "run") }),

        new("Data Structures in C#", "Programming", "Intermediate",
            "Pick the right collection for each job in C#.",
            new[] { "Arrays and lists", "Stacks and queues", "Dictionaries and sets", "Choosing a structure" },
            new[] { ("Which structure works first in, first out?", "Queue", "Stack", "Set"),
                    ("Which collection stores key and value pairs?", "Dictionary", "List", "Queue") }),

        new("Object Oriented Programming", "Programming", "Intermediate",
            "Understand the four ideas behind object oriented code.",
            new[] { "Classes and objects", "Encapsulation", "Inheritance", "Polymorphism" },
            new[] { ("Which idea hides the inner details of a class?", "Encapsulation", "Recursion", "Iteration"),
                    ("Which idea lets a class reuse another class?", "Inheritance", "Overloading", "Casting") }),

        new("Clean Code Practices", "Programming", "Advanced",
            "Write code that other people can read and change with ease.",
            new[] { "Meaningful names", "Small functions", "Comments and formatting", "Refactoring" },
            new[] { ("How many things should a good function do?", "One thing", "Many things", "Everything"),
                    ("What does refactoring change?", "The structure, not the behavior", "The behavior only", "The database") }),

        new("HTML and CSS Foundations", "Web Development", "Beginner",
            "Build and style your first web pages from scratch.",
            new[] { "Page structure", "Text, links and images", "CSS selectors", "Box model and layout" },
            new[] { ("Which tag creates a link?", "<a>", "<link>", "<href>"),
                    ("Which CSS property changes text color?", "color", "font-color", "text-style") }),

        new("Responsive Web Design", "Web Development", "Intermediate",
            "Make pages look great on phones, tablets and desktops.",
            new[] { "Fluid layouts", "Media queries", "Flexbox", "CSS Grid" },
            new[] { ("Which feature applies styles by screen size?", "Media query", "Selector", "Variable"),
                    ("Which layout system works in two dimensions?", "CSS Grid", "Float", "Inline") }),

        new("React Basics", "Web Development", "Intermediate",
            "Build interactive screens with components, props and state.",
            new[] { "Components and JSX", "Props", "State with hooks", "Lists and events" },
            new[] { ("Which hook stores state in a component?", "useState", "useFetch", "useMap"),
                    ("What do components receive from their parents?", "Props", "Cookies", "Routes") }),

        new("Node.js and Express", "Web Development", "Intermediate",
            "Create a server and an API with JavaScript.",
            new[] { "What Node.js is", "Routes and middleware", "Reading request data", "Connecting a database" },
            new[] { ("Which package manager comes with Node.js?", "npm", "pip", "nuget"),
                    ("What does middleware do in Express?", "Runs between the request and the response", "Draws the page", "Compiles the code") }),

        new("Web API with ASP.NET Core", "Web Development", "Advanced",
            "Design clean REST APIs with controllers, binding and status codes.",
            new[] { "Controllers and routes", "Model binding", "Status codes", "Authentication basics" },
            new[] { ("Which status code means created?", "201", "404", "500"),
                    ("Which attribute marks a class as an API controller?", "[ApiController]", "[Route]", "[Api]") }),

        new("PostgreSQL in Practice", "Database", "Intermediate",
            "Design tables, write joins and keep queries fast.",
            new[] { "Tables and keys", "Joins", "Indexes", "Backups" },
            new[] { ("Which clause filters rows?", "WHERE", "ORDER BY", "GROUP BY"),
                    ("What speeds up lookups on a column?", "An index", "A view", "A trigger") }),

        new("Entity Framework Core", "Database", "Intermediate",
            "Talk to your database with C# classes and LINQ.",
            new[] { "DbContext and DbSet", "Migrations", "Relationships", "Querying with LINQ" },
            new[] { ("Which command adds a migration?", "dotnet ef migrations add", "dotnet ef drop", "dotnet ef run"),
                    ("Which class represents a database session?", "DbContext", "DbSet", "Migration") }),

        new("MongoDB Basics", "Database", "Beginner",
            "Store and find data in a document database.",
            new[] { "Documents and collections", "Insert and find", "Updating data", "Schema design" },
            new[] { ("MongoDB stores data as what?", "Documents", "Rows", "Cells"),
                    ("A group of documents is called what?", "Collection", "Table", "Sheet") }),

        new("UI Design Principles", "Design", "Beginner",
            "Learn the rules that make an interface clear and pleasant.",
            new[] { "Visual hierarchy", "Color and contrast", "Typography", "Spacing and alignment" },
            new[] { ("What does contrast help with?", "Readability", "File size", "Loading speed"),
                    ("Which tool guides the eye to key content?", "Visual hierarchy", "Caching", "Routing") }),

        new("Figma for Beginners", "Design", "Beginner",
            "Design screens and clickable prototypes in Figma.",
            new[] { "The Figma workspace", "Frames and shapes", "Components", "Prototyping" },
            new[] { ("What is a reusable design element called?", "Component", "Layer", "Export"),
                    ("What links screens to show a flow?", "Prototype", "Grid", "Plugin") }),

        new("Data Analysis with Python", "Data Science", "Intermediate",
            "Clean, group and chart real data with pandas.",
            new[] { "Pandas basics", "Cleaning data", "Grouping and summaries", "Charts" },
            new[] { ("Which library works with data frames?", "pandas", "flask", "django"),
                    ("Which step fixes missing and wrong values?", "Data cleaning", "Deployment", "Compiling") }),

        new("Machine Learning Basics", "Data Science", "Advanced",
            "Understand how models learn and how to measure them.",
            new[] { "What machine learning is", "Training and testing", "Regression and classification", "Measuring accuracy" },
            new[] { ("What is the data used to teach a model called?", "Training data", "Test cases", "Source code"),
                    ("Which task predicts a category?", "Classification", "Compression", "Sorting") }),

        new("Git and GitHub", "Tools", "Beginner",
            "Track your work and team up with Git and GitHub.",
            new[] { "Commits and history", "Branches", "Pull requests", "Fixing conflicts" },
            new[] { ("Which command saves a snapshot?", "git commit", "git push", "git clone"),
                    ("Which command uploads commits to GitHub?", "git push", "git add", "git init") }),

        new("Docker for Developers", "Tools", "Intermediate",
            "Package your app so it runs the same everywhere.",
            new[] { "Images and containers", "Writing a Dockerfile", "Ports and volumes", "Docker Compose" },
            new[] { ("Which file defines how an image is built?", "Dockerfile", "README", "package.json"),
                    ("What is a running image called?", "Container", "Volume", "Layer") }),

        new("Software Testing Basics", "Tools", "Intermediate",
            "Catch bugs early with unit tests and good habits.",
            new[] { "Why we test", "Unit tests", "Test doubles", "Test coverage" },
            new[] { ("What does a unit test check?", "One small piece of code", "The whole system", "The network"),
                    ("What does coverage measure?", "How much code the tests run", "How fast code runs", "How big files are") }),

        new("Agile and Scrum", "Tools", "Beginner",
            "Learn how modern teams plan and ship software.",
            new[] { "Agile values", "Scrum roles", "Sprints and backlog", "Retrospectives" },
            new[] { ("How long is a typical sprint?", "One to four weeks", "One day", "One year"),
                    ("Who owns the product backlog?", "Product Owner", "Tester", "Designer") })
    };

    public static async Task AddMoreAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (await db.Courses.CountAsync() >= 20) return;

        var instructors = new List<ApplicationUser>();
        foreach (var (email, name) in new[]
        {
            ("instructor@lms.com", "Sara Khan"),
            ("ayesha@lms.com", "Ayesha Malik"),
            ("bilal@lms.com", "Bilal Ahmed"),
            ("hina@lms.com", "Hina Farooq")
        })
        {
            instructors.Add(await DbSeeder.CreateUserAsync(users, email, name, "Instructor@123", DbSeeder.InstructorRole));
        }

        var students = new List<ApplicationUser>();
        foreach (var (email, name) in new[]
        {
            ("usman@lms.com", "Usman Tariq"),
            ("zainab@lms.com", "Zainab Ali"),
            ("hamza@lms.com", "Hamza Sheikh"),
            ("maryam@lms.com", "Maryam Noor"),
            ("daniyal@lms.com", "Daniyal Khan"),
            ("fatima@lms.com", "Fatima Zahra"),
            ("saad@lms.com", "Saad Iqbal"),
            ("noor@lms.com", "Noor Fatima"),
            ("ali.hassan@lms.com", "Ali Hassan"),
            ("rabia@lms.com", "Rabia Aslam")
        })
        {
            students.Add(await DbSeeder.CreateUserAsync(users, email, name, "Student@123", DbSeeder.StudentRole));
        }

        var created = new List<Course>();

        for (var i = 0; i < Seeds.Length; i++)
        {
            var s = Seeds[i];

            var course = new Course
            {
                Title = s.Title,
                Description = s.Description,
                Category = s.Category,
                Level = s.Level,
                IsPublished = true,
                InstructorId = instructors[i % instructors.Count].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-(i + 1))
            };

            for (var l = 0; l < s.Lessons.Length; l++)
            {
                course.Lessons.Add(new Lesson
                {
                    Title = s.Lessons[l],
                    Order = l + 1,
                    Content = s.Lessons[l] + ". In this lesson you learn the main ideas of this topic for the course "
                        + s.Title + ". Read the notes, try the example on your own and then mark the lesson as complete."
                });
            }

            var quiz = new Quiz { Title = s.Title + " Quiz", PassMark = 50 };
            for (var q = 0; q < s.Quiz.Length; q++)
            {
                var item = s.Quiz[q];
                var options = new List<AnswerOption>
                {
                    new() { Text = item.Right, IsCorrect = true },
                    new() { Text = item.W1 },
                    new() { Text = item.W2 }
                };

                var shift = (i + q) % 3;
                options = options.Skip(shift).Concat(options.Take(shift)).ToList();

                quiz.Questions.Add(new Question { Text = item.Q, Options = options });
            }
            course.Quizzes.Add(quiz);

            course.Assignments.Add(new Assignment
            {
                Title = "Mini project: " + s.Title,
                Instructions = "Build a small example that uses what you learned in " + s.Title
                    + ". Explain your steps in your own words and describe one problem you solved.",
                DueDate = DateTime.UtcNow.AddDays(5 + i % 10),
                MaxScore = 100
            });

            created.Add(course);
        }

        db.Courses.AddRange(created);
        await db.SaveChangesAsync();

        for (var k = 0; k < students.Count; k++)
        {
            for (var ci = 0; ci < created.Count; ci++)
            {
                if ((ci + k) % 4 != 0) continue;

                var course = created[ci];
                var student = students[k];
                var lessons = course.Lessons.OrderBy(x => x.Order).ToList();
                var done = (k + ci) % (lessons.Count + 1);

                db.Enrollments.Add(new Enrollment
                {
                    CourseId = course.Id,
                    StudentId = student.Id,
                    EnrolledAt = DateTime.UtcNow.AddDays(-((k * 3 + ci) % 25)),
                    CompletedAt = done == lessons.Count ? DateTime.UtcNow.AddDays(-1) : (DateTime?)null
                });

                foreach (var lesson in lessons.Take(done))
                {
                    db.LessonProgresses.Add(new LessonProgress { LessonId = lesson.Id, StudentId = student.Id });
                }

                if (done >= 2)
                {
                    db.QuizAttempts.Add(new QuizAttempt
                    {
                        QuizId = course.Quizzes[0].Id,
                        StudentId = student.Id,
                        Score = (k + ci) % 3,
                        Total = 2,
                        TakenAt = DateTime.UtcNow.AddDays(-((k + ci) % 10))
                    });
                }

                if (done == lessons.Count || (k + ci) % 8 == 0)
                {
                    var graded = k % 2 == 0;
                    db.Submissions.Add(new Submission
                    {
                        AssignmentId = course.Assignments[0].Id,
                        StudentId = student.Id,
                        Content = "I built a small example and followed the lesson steps. The hardest part was the first setup, and I solved it by reading the notes again.",
                        SubmittedAt = DateTime.UtcNow.AddDays(-((k + ci) % 6)),
                        Score = graded ? 70 + (k * 3) % 30 : null,
                        Feedback = graded ? "Good work. Keep it up." : null
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }
}