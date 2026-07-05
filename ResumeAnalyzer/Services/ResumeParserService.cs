using System.Text;
using System.Text.RegularExpressions;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.ViewModels;

namespace ResumeAnalyzer.Services
{
    /// <summary>
    /// Parses uploaded resume files and extracts structured information.
    /// </summary>
    public class ResumeParserService
    {
        private readonly List<string> _knownSkills = new()
        {
            "ASP.NET Core","ASP.NET","C#","VB.NET",".NET",
            "JavaScript","TypeScript","Python","Java","PHP","Ruby","Go","Rust","Swift","Kotlin",
            "React","Angular","Vue.js","Next.js","Node.js","Express","Django","Flask","Laravel",
            "SQL","MySQL","PostgreSQL","MongoDB","SQLite","Oracle","Redis","Elasticsearch",
            "HTML","CSS","Bootstrap","Tailwind CSS","SASS","SCSS",
            "Docker","Kubernetes","Jenkins","CI/CD","GitHub Actions","Terraform",
            "Azure","AWS","GCP","Google Cloud",
            "Machine Learning","Deep Learning","TensorFlow","PyTorch","Scikit-learn","NLP","AI",
            "Git","GitHub","GitLab","Bitbucket",
            "REST API","GraphQL","Microservices","SOAP","gRPC",
            "Agile","Scrum","Kanban","JIRA","Confluence",
            "Linux","Bash","PowerShell","Shell Scripting",
            "Power BI","Tableau","Data Analysis","Pandas","NumPy","Matplotlib",
            "Unity","Unreal Engine","OpenGL","Vulkan",
            "Blockchain","Solidity","Smart Contracts",
            "Selenium","Jest","NUnit","xUnit","Mocha","Cypress",
            "Spring Boot","Hibernate","Maven","Gradle",
            "MATLAB","R","Scala","Haskell","Perl",
            "WordPress","Shopify","Magento","Drupal",
            "Figma","Adobe XD","Sketch","Photoshop","Illustrator",
            "Project Management","Communication","Leadership","Problem Solving","Teamwork"
        };

        /// <summary>Extract raw text from a PDF or DOCX file.</summary>
        public async Task<string> ExtractTextAsync(string filePath, string fileType)
        {
            try
            {
                if (fileType.ToLower() == "pdf")
                    return await ExtractFromPdfAsync(filePath);
                else if (fileType.ToLower() == "docx")
                    return await ExtractFromDocxAsync(filePath);
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> ExtractFromPdfAsync(string filePath)
        {
            await Task.CompletedTask;
            try
            {
                var sb = new StringBuilder();
                using var doc = UglyToad.PdfPig.PdfDocument.Open(filePath);
                foreach (var page in doc.GetPages())
                    sb.AppendLine(page.Text);
                return sb.ToString();
            }
            catch { return string.Empty; }
        }

        private async Task<string> ExtractFromDocxAsync(string filePath)
        {
            await Task.CompletedTask;
            try
            {
                var sb = new StringBuilder();
                using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(filePath, false);
                var body = doc.MainDocumentPart?.Document?.Body;
                if (body != null)
                    foreach (var para in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                        sb.AppendLine(para.InnerText);
                return sb.ToString();
            }
            catch { return string.Empty; }
        }

        /// <summary>Detect all known skills present in the resume text.</summary>
        public List<string> DetectSkills(string text)
        {
            var detected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var upper = text.ToUpper();
            foreach (var skill in _knownSkills)
                if (upper.Contains(skill.ToUpper()))
                    detected.Add(skill);
            return detected.OrderBy(s => s).ToList();
        }

        /// <summary>Extract email address from text.</summary>
        public string? ExtractEmail(string text)
        {
            var match = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value : null;
        }

        /// <summary>Extract phone number from text.</summary>
        public string? ExtractPhone(string text)
        {
            var match = Regex.Match(text, @"(\+?\d{1,3}[\s.-]?)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}");
            return match.Success ? match.Value.Trim() : null;
        }

        /// <summary>Attempt to extract full name from first non-empty line.</summary>
        public string? ExtractName(string text)
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Take(5))
            {
                var clean = line.Trim();
                if (clean.Length > 3 && clean.Length < 60 && !clean.Contains('@') && Regex.IsMatch(clean, @"^[A-Za-z\s]+$"))
                    return clean;
            }
            return null;
        }

        /// <summary>Calculate a 0-100 resume quality score.</summary>
        public int CalculateResumeScore(string text, List<string> skills)
        {
            int score = 0;

            // Skills (40 pts max)
            score += Math.Min(skills.Count * 2, 40);

            // Contact info (15 pts)
            if (ExtractEmail(text) != null) score += 8;
            if (ExtractPhone(text) != null) score += 7;

            // Sections present (30 pts)
            var upper = text.ToUpper();
            if (upper.Contains("EXPERIENCE") || upper.Contains("WORK HISTORY")) score += 10;
            if (upper.Contains("EDUCATION")) score += 8;
            if (upper.Contains("PROJECT") || upper.Contains("PORTFOLIO")) score += 7;
            if (upper.Contains("CERTIF")) score += 5;

            // Length (15 pts)
            var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount >= 200) score += 5;
            if (wordCount >= 400) score += 5;
            if (wordCount >= 600) score += 5;

            return Math.Min(score, 100);
        }

        /// <summary>Calculate an ATS compatibility score.</summary>
        public int CalculateAtsScore(string text, List<string> skills)
        {
            int score = 0;
            var upper = text.ToUpper();

            // Keywords density (30 pts)
            score += Math.Min(skills.Count * 2, 30);

            // Standard section headings (40 pts)
            string[] headings = { "SUMMARY", "OBJECTIVE", "EXPERIENCE", "EDUCATION", "SKILLS", "CERTIF", "PROJECT" };
            foreach (var h in headings)
                if (upper.Contains(h)) score += 5;
            score = Math.Min(score, 40 + 30); // cap combined

            // No tables / special characters (assumed clean text) (15 pts)
            var ratio = (double)Regex.Matches(text, @"[A-Za-z0-9 ]").Count / Math.Max(text.Length, 1);
            if (ratio > 0.85) score += 15;

            // Bullet-point style (15 pts) — presence of '-' or '•'
            if (text.Contains('•') || Regex.Matches(text, @"^\s*[-*]", RegexOptions.Multiline).Count > 3)
                score += 15;

            return Math.Min(score, 100);
        }

        /// <summary>Generate improvement suggestions based on resume content.</summary>
        public List<SuggestionViewModel> GenerateSuggestions(string text, List<string> skills)
        {
            var suggestions = new List<SuggestionViewModel>();
            var upper = text.ToUpper();

            if (skills.Count < 5)
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-code", Title = "Add More Technical Skills", Description = "Your resume has fewer than 5 skills listed. Add relevant technical and soft skills to improve match rates.", Priority = "High", BadgeColor = "danger" });

            if (!upper.Contains("SUMMARY") && !upper.Contains("OBJECTIVE"))
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-file-alt", Title = "Add a Professional Summary", Description = "Include a 2-3 sentence professional summary at the top highlighting your experience and goals.", Priority = "High", BadgeColor = "danger" });

            if (!upper.Contains("PROJECT"))
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-laptop-code", Title = "Add a Projects Section", Description = "List personal or academic projects with technologies used and impact to demonstrate practical experience.", Priority = "Medium", BadgeColor = "warning" });

            if (!upper.Contains("CERTIF"))
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-certificate", Title = "Add Certifications", Description = "Industry certifications (AWS, Azure, Google) significantly boost ATS scores and recruiter attention.", Priority = "Medium", BadgeColor = "warning" });

            if (!Regex.IsMatch(text, @"\b(increased|improved|reduced|achieved|delivered|built|led|managed)\b", RegexOptions.IgnoreCase))
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-chart-line", Title = "Add Measurable Achievements", Description = "Quantify your achievements with numbers. E.g., 'Reduced load time by 40%' or 'Led a team of 5 developers'.", Priority = "High", BadgeColor = "danger" });

            if (!upper.Contains("LINKEDIN") && !upper.Contains("GITHUB") && !upper.Contains("PORTFOLIO"))
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-link", Title = "Add Online Profiles", Description = "Include links to LinkedIn, GitHub, or portfolio website to make your resume stand out.", Priority = "Medium", BadgeColor = "warning" });

            var wordCount = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            if (wordCount < 300)
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-expand-alt", Title = "Expand Resume Content", Description = "Your resume appears too brief. Add more details about your experience, responsibilities, and achievements.", Priority = "Medium", BadgeColor = "warning" });

            if (!Regex.IsMatch(text, @"\b(20\d{2}|19\d{2})\b"))
                suggestions.Add(new SuggestionViewModel { Icon = "fas fa-calendar-alt", Title = "Add Dates to Experience", Description = "Include start and end dates for each position to help recruiters understand your career timeline.", Priority = "Low", BadgeColor = "info" });

            suggestions.Add(new SuggestionViewModel { Icon = "fas fa-spell-check", Title = "Review Grammar & Spelling", Description = "Ensure your resume has no typos or grammatical errors. Use active voice and consistent tense.", Priority = "Low", BadgeColor = "info" });

            return suggestions;
        }
    }
}
