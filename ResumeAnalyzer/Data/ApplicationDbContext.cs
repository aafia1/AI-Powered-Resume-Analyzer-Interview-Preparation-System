using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Resume> Resumes { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<ResumeSkill> ResumeSkills { get; set; }
        public DbSet<JobDescription> JobDescriptions { get; set; }
        public DbSet<AnalysisResult> AnalysisResults { get; set; }
        public DbSet<InterviewQuestion> InterviewQuestions { get; set; }
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ResumeSkill>()
                .HasKey(rs => new { rs.ResumeId, rs.SkillId });

            builder.Entity<ResumeSkill>()
                .HasOne(rs => rs.Resume)
                .WithMany(r => r.ResumeSkills)
                .HasForeignKey(rs => rs.ResumeId);

            builder.Entity<ResumeSkill>()
                .HasOne(rs => rs.Skill)
                .WithMany(s => s.ResumeSkills)
                .HasForeignKey(rs => rs.SkillId);

            // Seed Skills
            builder.Entity<Skill>().HasData(
                new Skill { Id = 1, Name = "ASP.NET Core", Category = "Backend", Proficiency = "Advanced" },
                new Skill { Id = 2, Name = "C#", Category = "Backend", Proficiency = "Advanced" },
                new Skill { Id = 3, Name = "JavaScript", Category = "Frontend", Proficiency = "Intermediate" },
                new Skill { Id = 4, Name = "Python", Category = "Backend", Proficiency = "Intermediate" },
                new Skill { Id = 5, Name = "SQL", Category = "Database", Proficiency = "Advanced" },
                new Skill { Id = 6, Name = "React", Category = "Frontend", Proficiency = "Intermediate" },
                new Skill { Id = 7, Name = "HTML", Category = "Frontend", Proficiency = "Beginner" },
                new Skill { Id = 8, Name = "CSS", Category = "Frontend", Proficiency = "Beginner" },
                new Skill { Id = 9, Name = "Machine Learning", Category = "AI", Proficiency = "Advanced" },
                new Skill { Id = 10, Name = "Docker", Category = "DevOps", Proficiency = "Intermediate" },
                new Skill { Id = 11, Name = "Git", Category = "DevOps", Proficiency = "Beginner" },
                new Skill { Id = 12, Name = "Azure", Category = "Cloud", Proficiency = "Advanced" },
                new Skill { Id = 13, Name = "Node.js", Category = "Backend", Proficiency = "Intermediate" },
                new Skill { Id = 14, Name = "Angular", Category = "Frontend", Proficiency = "Intermediate" },
                new Skill { Id = 15, Name = "Vue.js", Category = "Frontend", Proficiency = "Intermediate" },
                new Skill { Id = 16, Name = "TypeScript", Category = "Frontend", Proficiency = "Intermediate" },
                new Skill { Id = 17, Name = "MongoDB", Category = "Database", Proficiency = "Intermediate" },
                new Skill { Id = 18, Name = "PostgreSQL", Category = "Database", Proficiency = "Advanced" },
                new Skill { Id = 19, Name = "REST API", Category = "Backend", Proficiency = "Advanced" },
                new Skill { Id = 20, Name = "Microservices", Category = "Architecture", Proficiency = "Advanced" },
                new Skill { Id = 21, Name = "Kubernetes", Category = "DevOps", Proficiency = "Advanced" },
                new Skill { Id = 22, Name = "AWS", Category = "Cloud", Proficiency = "Advanced" },
                new Skill { Id = 23, Name = "Java", Category = "Backend", Proficiency = "Intermediate" },
                new Skill { Id = 24, Name = "Spring Boot", Category = "Backend", Proficiency = "Intermediate" },
                new Skill { Id = 25, Name = "TensorFlow", Category = "AI", Proficiency = "Advanced" },
                new Skill { Id = 26, Name = "Deep Learning", Category = "AI", Proficiency = "Advanced" },
                new Skill { Id = 27, Name = "Data Analysis", Category = "Data", Proficiency = "Intermediate" },
                new Skill { Id = 28, Name = "Power BI", Category = "Data", Proficiency = "Intermediate" },
                new Skill { Id = 29, Name = "Agile", Category = "Methodology", Proficiency = "Beginner" },
                new Skill { Id = 30, Name = "Scrum", Category = "Methodology", Proficiency = "Beginner" }
            );

            // Seed Interview Questions
            builder.Entity<InterviewQuestion>().HasData(
                // ASP.NET
                new InterviewQuestion { Id = 1, SkillName = "ASP.NET Core", Question = "Explain the MVC architecture pattern in ASP.NET Core.", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 2, SkillName = "ASP.NET Core", Question = "What is middleware in ASP.NET Core and how does it work?", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 3, SkillName = "ASP.NET Core", Question = "Explain dependency injection and how it's implemented in ASP.NET Core.", Level = "Advanced", Category = "Technical" },
                // C#
                new InterviewQuestion { Id = 4, SkillName = "C#", Question = "What is the difference between value types and reference types in C#?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 5, SkillName = "C#", Question = "Explain async/await and how they work in C#.", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 6, SkillName = "C#", Question = "What are LINQ expressions and when would you use them?", Level = "Advanced", Category = "Technical" },
                // SQL
                new InterviewQuestion { Id = 7, SkillName = "SQL", Question = "What is normalization in SQL and what are its normal forms?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 8, SkillName = "SQL", Question = "Explain the difference between INNER JOIN, LEFT JOIN, and RIGHT JOIN.", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 9, SkillName = "SQL", Question = "What are stored procedures and when should you use them?", Level = "Advanced", Category = "Technical" },
                // JavaScript
                new InterviewQuestion { Id = 10, SkillName = "JavaScript", Question = "What is the difference between let, var, and const in JavaScript?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 11, SkillName = "JavaScript", Question = "Explain closures and how they work in JavaScript.", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 12, SkillName = "JavaScript", Question = "What are Promises and how do they differ from async/await?", Level = "Advanced", Category = "Technical" },
                // Python
                new InterviewQuestion { Id = 13, SkillName = "Python", Question = "What are Python decorators and how are they used?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 14, SkillName = "Python", Question = "Explain list comprehensions and generators in Python.", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 15, SkillName = "Python", Question = "What is GIL in Python and how does it affect multithreading?", Level = "Advanced", Category = "Technical" },
                // React
                new InterviewQuestion { Id = 16, SkillName = "React", Question = "What is the virtual DOM and how does React use it?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 17, SkillName = "React", Question = "Explain React hooks and the useEffect hook.", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 18, SkillName = "React", Question = "What is Redux and when would you use it over React Context?", Level = "Advanced", Category = "Technical" },
                // Machine Learning
                new InterviewQuestion { Id = 19, SkillName = "Machine Learning", Question = "What is the difference between supervised and unsupervised learning?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 20, SkillName = "Machine Learning", Question = "Explain overfitting and how to prevent it.", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 21, SkillName = "Machine Learning", Question = "What is gradient descent and how does it work?", Level = "Advanced", Category = "Technical" },
                // Docker
                new InterviewQuestion { Id = 22, SkillName = "Docker", Question = "What is Docker and how does it differ from a virtual machine?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 23, SkillName = "Docker", Question = "What is a Dockerfile and how do you create a Docker image?", Level = "Intermediate", Category = "Technical" },
                new InterviewQuestion { Id = 24, SkillName = "Docker", Question = "Explain Docker Compose and multi-container orchestration.", Level = "Advanced", Category = "Technical" },
                // Git
                new InterviewQuestion { Id = 25, SkillName = "Git", Question = "What is the difference between git merge and git rebase?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 26, SkillName = "Git", Question = "Explain the Git branching strategy for large teams.", Level = "Intermediate", Category = "Technical" },
                // REST API
                new InterviewQuestion { Id = 27, SkillName = "REST API", Question = "What are the principles of RESTful API design?", Level = "Beginner", Category = "Conceptual" },
                new InterviewQuestion { Id = 28, SkillName = "REST API", Question = "How do you handle authentication in a REST API (JWT vs OAuth)?", Level = "Intermediate", Category = "Technical" }
            );
        }
    }
}
