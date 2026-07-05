using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using ResumeAnalyzer.ViewModels;

namespace ResumeAnalyzer.Services
{
    /// <summary>
    /// Generates downloadable PDF analysis reports using iText7.
    /// </summary>
    public class PdfReportService
    {
        private readonly IWebHostEnvironment _env;

        public PdfReportService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GenerateReport(ResumeAnalysisViewModel analysis, string userName)
        {
            var reportsDir = Path.Combine(_env.WebRootPath, "uploads", "reports");
            Directory.CreateDirectory(reportsDir);

            var fileName = $"report_{analysis.ResumeId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(reportsDir, fileName);

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var darkBlue = new DeviceRgb(30, 58, 138);
            var accent = new DeviceRgb(99, 102, 241);
            var lightGray = new DeviceRgb(248, 250, 252);
            var textGray = new DeviceRgb(75, 85, 99);

            // ── Header ────────────────────────────────────────────────────────
            var header = new Paragraph("AI RESUME ANALYSIS REPORT")
                .SetFont(boldFont).SetFontSize(22).SetFontColor(darkBlue)
                .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(4);
            doc.Add(header);

            doc.Add(new Paragraph($"Generated for: {userName}  |  {DateTime.UtcNow:MMMM dd, yyyy HH:mm} UTC")
                .SetFont(regularFont).SetFontSize(10).SetFontColor(textGray)
                .SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(20));

            // ── Score Cards ───────────────────────────────────────────────────
            AddSectionTitle(doc, boldFont, darkBlue, "📊 SCORE OVERVIEW");

            var scoreTable = new Table(3).UseAllAvailableWidth().SetMarginBottom(20);
            AddScoreCell(scoreTable, boldFont, regularFont, accent, "Resume Score", $"{analysis.ResumeScore}/100");
            AddScoreCell(scoreTable, boldFont, regularFont, new DeviceRgb(16, 185, 129), "ATS Score", $"{analysis.AtsScore}/100");
            AddScoreCell(scoreTable, boldFont, regularFont, new DeviceRgb(245, 158, 11), "Skills Found", $"{analysis.DetectedSkills.Count}");
            doc.Add(scoreTable);

            // ── Contact Info ──────────────────────────────────────────────────
            if (!string.IsNullOrEmpty(analysis.ParsedName) || !string.IsNullOrEmpty(analysis.ParsedEmail))
            {
                AddSectionTitle(doc, boldFont, darkBlue, "👤 EXTRACTED CONTACT INFORMATION");
                if (!string.IsNullOrEmpty(analysis.ParsedName))
                    doc.Add(BulletLine(regularFont, textGray, $"Name: {analysis.ParsedName}"));
                if (!string.IsNullOrEmpty(analysis.ParsedEmail))
                    doc.Add(BulletLine(regularFont, textGray, $"Email: {analysis.ParsedEmail}"));
                if (!string.IsNullOrEmpty(analysis.ParsedPhone))
                    doc.Add(BulletLine(regularFont, textGray, $"Phone: {analysis.ParsedPhone}"));
                doc.Add(new Paragraph("\n"));
            }

            // ── Skills ────────────────────────────────────────────────────────
            AddSectionTitle(doc, boldFont, darkBlue, "🛠️ DETECTED SKILLS");
            if (analysis.DetectedSkills.Any())
                doc.Add(new Paragraph(string.Join("  •  ", analysis.DetectedSkills))
                    .SetFont(regularFont).SetFontSize(11).SetFontColor(textGray).SetMarginBottom(16));
            else
                doc.Add(new Paragraph("No skills detected.").SetFont(regularFont).SetFontColor(textGray).SetMarginBottom(16));

            // ── Suggestions ───────────────────────────────────────────────────
            AddSectionTitle(doc, boldFont, darkBlue, "💡 IMPROVEMENT SUGGESTIONS");
            foreach (var s in analysis.Suggestions)
            {
                doc.Add(new Paragraph($"[{s.Priority.ToUpper()}] {s.Title}")
                    .SetFont(boldFont).SetFontSize(11).SetFontColor(accent).SetMarginBottom(2));
                doc.Add(new Paragraph(s.Description)
                    .SetFont(regularFont).SetFontSize(10).SetFontColor(textGray).SetMarginBottom(8));
            }

            // ── Interview Questions ────────────────────────────────────────────
            if (analysis.InterviewQuestions.Any())
            {
                AddSectionTitle(doc, boldFont, darkBlue, "🎤 SAMPLE INTERVIEW QUESTIONS");
                foreach (var q in analysis.InterviewQuestions.Take(10))
                {
                    doc.Add(new Paragraph($"[{q.Level}] ({q.Skill}) {q.Question}")
                        .SetFont(regularFont).SetFontSize(11).SetFontColor(textGray).SetMarginBottom(6));
                }
            }

            // ── Footer ────────────────────────────────────────────────────────
            doc.Add(new Paragraph("\n\nGenerated by AI Resume Analyzer — University Semester Project")
                .SetFont(regularFont).SetFontSize(9).SetFontColor(new DeviceRgb(156, 163, 175))
                .SetTextAlignment(TextAlignment.CENTER));

            return $"/uploads/reports/{fileName}";
        }

        private static void AddSectionTitle(Document doc, PdfFont font, DeviceRgb color, string title)
        {
            doc.Add(new Paragraph(title).SetFont(font).SetFontSize(14).SetFontColor(color).SetMarginBottom(8));
        }

        private static Paragraph BulletLine(PdfFont font, DeviceRgb color, string text) =>
            new Paragraph($"  ▸  {text}").SetFont(font).SetFontSize(11).SetFontColor(color).SetMarginBottom(4);

        private static void AddScoreCell(Table table, PdfFont bold, PdfFont regular, DeviceRgb color, string label, string value)
        {
            var cell = new Cell()
                .SetBackgroundColor(color)
                .SetPadding(14)
                .SetTextAlignment(TextAlignment.CENTER);
            cell.Add(new Paragraph(value).SetFont(bold).SetFontSize(24).SetFontColor(ColorConstants.WHITE));
            cell.Add(new Paragraph(label).SetFont(regular).SetFontSize(11).SetFontColor(ColorConstants.WHITE));
            table.AddCell(cell);
        }
    }
}
