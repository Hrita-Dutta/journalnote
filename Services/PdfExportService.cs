using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JournalNote.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using VerticalAlignment = iText.Layout.Properties.VerticalAlignment;

namespace JournalNote.Services
{
    public class PdfExportService
    {
        private readonly DatabaseService _databaseService;

        public PdfExportService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<string> ExportToPdfAsync(ExportRequest request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine(">>> PDF EXPORT STARTED");
                System.Diagnostics.Debug.WriteLine($">>> Start Date: {request.StartDate:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($">>> End Date: {request.EndDate:yyyy-MM-dd}");

                // Get all entries
                var allEntries = await _databaseService.GetAllEntriesAsync();
                System.Diagnostics.Debug.WriteLine($">>> Total entries: {allEntries?.Count ?? 0}");

                if (allEntries == null || allEntries.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine(">>> No entries in database");
                    return null;
                }

                // Filter by date range
                var filtered = allEntries
                    .Where(e => !string.IsNullOrEmpty(e.Date))
                    .Where(e =>
                    {
                        try
                        {
                            var date = DateTime.Parse(e.Date).Date;
                            return date >= request.StartDate.Date && date <= request.EndDate.Date;
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .OrderBy(e => DateTime.Parse(e.Date))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($">>> Filtered entries: {filtered.Count}");

                if (filtered.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine(">>> No entries in date range");
                    return null;
                }

                // Get moods and tags
                var moods = await _databaseService.GetAllMoodsAsync() ?? new List<Mood>();
                var tags = await _databaseService.GetAllTagsAsync() ?? new List<Tag>();

                // Create PDF file
                var fileName = $"Journal_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                System.Diagnostics.Debug.WriteLine($">>> Creating PDF: {filePath}");

                // Create fonts
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Generate PDF
                using (var writer = new PdfWriter(filePath))
                using (var pdfDoc = new PdfDocument(writer))
                using (var document = new Document(pdfDoc))
                {
                    // Set margins
                    document.SetMargins(50, 50, 50, 50);

                    // ===== HEADER =====
                    var titlePara = new Paragraph("My Journal Export")
                        .SetFont(boldFont)
                        .SetFontSize(24)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(10);
                    document.Add(titlePara);

                    var datePara = new Paragraph($"Exported: {DateTime.Now:MMMM dd, yyyy h:mm tt}")
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginBottom(20);
                    document.Add(datePara);

                    // ===== SUMMARY =====
                    var summaryPara = new Paragraph()
                        .SetFont(normalFont)
                        .SetFontSize(10)
                        .SetMarginBottom(20);
                    
                    summaryPara.Add(new Text("Date Range: ").SetFont(boldFont));
                    summaryPara.Add($"{request.StartDate:MMM dd, yyyy} - {request.EndDate:MMM dd, yyyy}\n");
                    summaryPara.Add(new Text("Total Entries: ").SetFont(boldFont));
                    summaryPara.Add($"{filtered.Count}");
                    
                    document.Add(summaryPara);

                    // Horizontal line
                    var lineSeparator = new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine());
                    document.Add(lineSeparator);
                    document.Add(new Paragraph("\n"));

                    // ===== ENTRIES =====
                    foreach (var entry in filtered)
                    {
                        // Date
                        var entryDate = SafeGetDate(entry.Date);
                        var dateParagraph = new Paragraph(entryDate)
                            .SetFont(boldFont)
                            .SetFontSize(10)
                            .SetFontColor(ColorConstants.BLUE)
                            .SetMarginBottom(5);
                        document.Add(dateParagraph);

                        // Title
                        var title = string.IsNullOrWhiteSpace(entry.Title) ? "Untitled" : entry.Title;
                        var titleParagraph = new Paragraph(title)
                            .SetFont(boldFont)
                            .SetFontSize(16)
                            .SetMarginBottom(8);
                        document.Add(titleParagraph);

                        // Mood
                        if (request.IncludeMoods && entry.PrimaryMoodId.HasValue)
                        {
                            var mood = moods.FirstOrDefault(m => m.Id == entry.PrimaryMoodId.Value);
                            if (mood != null)
                            {
                                var moodText = $"Mood: {mood.Name}";

                                // Secondary moods
                                if (!string.IsNullOrEmpty(entry.SecondaryMoodIds))
                                {
                                    var secondaryMoods = new List<string>();
                                    var moodIds = entry.SecondaryMoodIds.Split(',')
                                        .Select(id => int.TryParse(id.Trim(), out int result) ? result : 0)
                                        .Where(id => id > 0);

                                    foreach (var moodId in moodIds)
                                    {
                                        var sMood = moods.FirstOrDefault(m => m.Id == moodId);
                                        if (sMood != null && !string.IsNullOrEmpty(sMood.Name))
                                            secondaryMoods.Add(sMood.Name);
                                    }

                                    if (secondaryMoods.Any())
                                        moodText += $" (Also: {string.Join(", ", secondaryMoods)})";
                                }

                                var moodParagraph = new Paragraph(moodText)
                                    .SetFont(normalFont)
                                    .SetFontSize(9)
                                    .SetFontColor(ColorConstants.DARK_GRAY)
                                    .SetMarginBottom(5);
                                document.Add(moodParagraph);
                            }
                        }

                        // Tags
                        if (request.IncludeTags && !string.IsNullOrEmpty(entry.TagIds))
                        {
                            var entryTags = new List<string>();
                            var tagIds = entry.TagIds.Split(',')
                                .Select(id => int.TryParse(id.Trim(), out int result) ? result : 0)
                                .Where(id => id > 0);

                            foreach (var tagId in tagIds)
                            {
                                var tag = tags.FirstOrDefault(t => t.Id == tagId);
                                if (tag != null && !string.IsNullOrEmpty(tag.Name))
                                    entryTags.Add(tag.Name);
                            }

                            if (entryTags.Any())
                            {
                                var tagParagraph = new Paragraph($"Tags: {string.Join(", ", entryTags)}")
                                    .SetFont(normalFont)
                                    .SetFontSize(9)
                                    .SetFontColor(ColorConstants.DARK_GRAY)
                                    .SetMarginBottom(8);
                                document.Add(tagParagraph);
                            }
                        }

                        // Content
                        var content = SafeGetContent(entry.Content);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var contentParagraph = new Paragraph(content)
                                .SetFont(normalFont)
                                .SetFontSize(11)
                                .SetMarginBottom(15);
                            document.Add(contentParagraph);
                        }

                        // Separator
                        document.Add(new LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.DottedLine()));
                        document.Add(new Paragraph("\n").SetMarginBottom(10));
                    }

                    // ===== FOOTER (Page numbers) =====
                    int totalPages = pdfDoc.GetNumberOfPages();
                    for (int i = 1; i <= totalPages; i++)
                    {
                        var page = pdfDoc.GetPage(i);
                        var pageSize = page.GetPageSize();
                        
                        var footerText = $"Page {i} of {totalPages}";
                        var canvas = new PdfCanvas(page);
                        
                        var footerPara = new Paragraph(footerText)
                            .SetFont(normalFont)
                            .SetFontSize(9);
                        
                        new Canvas(canvas, pageSize)
                            .ShowTextAligned(footerPara, pageSize.GetWidth() / 2, 30, i, 
                                TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0)
                            .Close();
                    }
                }

                System.Diagnostics.Debug.WriteLine(">>> PDF CREATED SUCCESSFULLY!");
                System.Diagnostics.Debug.WriteLine($">>> File: {filePath}");
                System.Diagnostics.Debug.WriteLine("========================================");
                
                return filePath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("========================================");
                System.Diagnostics.Debug.WriteLine(">>> PDF EXPORT ERROR");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                
                System.Diagnostics.Debug.WriteLine("========================================");
                
                throw;
            }
        }

        private string SafeGetDate(string date)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(date))
                    return DateTime.Now.ToString("MMMM dd, yyyy");

                return DateTime.Parse(date).ToString("MMMM dd, yyyy");
            }
            catch
            {
                return DateTime.Now.ToString("MMMM dd, yyyy");
            }
        }

        private string SafeGetContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return "";

            try
            {
                // Remove HTML tags
                var cleaned = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);
                
                // Decode HTML entities
                cleaned = System.Net.WebUtility.HtmlDecode(cleaned);
                
                return cleaned;
            }
            catch
            {
                return content;
            }
        }
    }
}