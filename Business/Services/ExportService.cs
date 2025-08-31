using ProjectControlsReportingTool.API.Business.Interfaces;
using ProjectControlsReportingTool.API.Models.DTOs;
using ProjectControlsReportingTool.API.Models.Enums;
using ProjectControlsReportingTool.API.Models.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;
using System.Globalization;
using OpenXmlDocument = DocumentFormat.OpenXml.Packaging.WordprocessingDocument;
using OpenXmlWordDocument = DocumentFormat.OpenXml.Wordprocessing.Document;
using OpenXmlBody = DocumentFormat.OpenXml.Wordprocessing.Body;
using OpenXmlParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using OpenXmlRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using OpenXmlText = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace ProjectControlsReportingTool.API.Business.Services
{
    public class ExportService : IExportService
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ExportService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly string _exportsPath;

        public ExportService(
            IReportService reportService,
            ILogger<ExportService> logger,
            IWebHostEnvironment environment)
        {
            _reportService = reportService;
            _logger = logger;
            _environment = environment;
            _exportsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "exports");
            
            // Ensure exports directory exists
            Directory.CreateDirectory(_exportsPath);
            
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Core Export Methods

        public async Task<ExportResultDto> ExportReportsAsync(ExportRequestDto request, Guid userId, UserRole userRole, Department userDepartment)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                
                // Validate request
                if (!await ValidateExportRequestAsync(request, userId, userRole))
                {
                    return new ExportResultDto { Success = false, ErrorMessage = "Invalid export request" };
                }

                // Get reports based on filter or specific IDs
                var reports = new List<ReportDetailDto>();
                
                if (request.SpecificReportIds?.Any() == true)
                {
                    foreach (var reportId in request.SpecificReportIds)
                    {
                        var report = await _reportService.GetReportByIdAsync(reportId, userId, userRole);
                        if (report != null)
                            reports.Add(report);
                    }
                }
                else if (request.ReportFilter != null)
                {
                    var summaryReports = await _reportService.GetReportsAsync(request.ReportFilter, userId, userRole, userDepartment);
                    foreach (var summary in summaryReports)
                    {
                        var report = await _reportService.GetReportByIdAsync(summary.Id, userId, userRole);
                        if (report != null)
                            reports.Add(report);
                    }
                }

                if (!reports.Any())
                {
                    return new ExportResultDto { Success = false, ErrorMessage = "No reports found for export" };
                }

                // Generate export based on format
                byte[] fileData;
                string contentType;
                string fileExtension;

                switch (request.Format)
                {
                    case ExportFormat.PDF:
                        fileData = await ExportReportsToPdfAsync(reports, request.Template);
                        contentType = "application/pdf";
                        fileExtension = ".pdf";
                        break;
                    case ExportFormat.Excel:
                        fileData = await ExportReportsToExcelAsync(reports, request.Template);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = ".xlsx";
                        break;
                    case ExportFormat.Word:
                        fileData = await ExportReportsToWordAsync(reports, request.Template);
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        fileExtension = ".docx";
                        break;
                    case ExportFormat.CSV:
                        fileData = await ExportReportsToCsvAsync(reports);
                        contentType = "text/csv";
                        fileExtension = ".csv";
                        break;
                    default:
                        return new ExportResultDto { Success = false, ErrorMessage = "Unsupported export format" };
                }

                // Save file
                var fileName = request.FileName ?? $"Reports_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";
                if (!fileName.EndsWith(fileExtension))
                    fileName += fileExtension;

                var filePath = Path.Combine(_exportsPath, fileName);
                await File.WriteAllBytesAsync(filePath, fileData);

                var processingTime = DateTime.UtcNow - startTime;

                return new ExportResultDto
                {
                    Success = true,
                    FileName = fileName,
                    ContentType = contentType,
                    FileSizeBytes = fileData.Length,
                    Format = request.Format,
                    Type = ExportType.Reports,
                    RecordCount = reports.Count,
                    DownloadUrl = $"/api/export/download/{Path.GetFileNameWithoutExtension(fileName)}",
                    ProcessingTime = processingTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting reports for user {UserId}", userId);
                return new ExportResultDto { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<ExportResultDto> ExportStatisticsAsync(ExportRequestDto request, Guid userId, UserRole userRole, Department userDepartment)
        {
            try
            {
                var startTime = DateTime.UtcNow;

                // Get statistics
                var filter = request.StatisticsFilter ?? new StatisticsFilterDto();
                var statistics = await _reportService.GetReportStatisticsAsync(filter, userId, userRole, userDepartment);

                // Generate export based on format
                byte[] fileData;
                string contentType;
                string fileExtension;

                switch (request.Format)
                {
                    case ExportFormat.PDF:
                        fileData = await ExportStatisticsToPdfAsync(statistics, request.Template);
                        contentType = "application/pdf";
                        fileExtension = ".pdf";
                        break;
                    case ExportFormat.Excel:
                        fileData = await ExportStatisticsToExcelAsync(statistics, request.Template);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileExtension = ".xlsx";
                        break;
                    default:
                        return new ExportResultDto { Success = false, ErrorMessage = "Unsupported format for statistics export" };
                }

                // Save file
                var fileName = request.FileName ?? $"Statistics_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}{fileExtension}";
                if (!fileName.EndsWith(fileExtension))
                    fileName += fileExtension;

                var filePath = Path.Combine(_exportsPath, fileName);
                await File.WriteAllBytesAsync(filePath, fileData);

                var processingTime = DateTime.UtcNow - startTime;

                return new ExportResultDto
                {
                    Success = true,
                    FileName = fileName,
                    ContentType = contentType,
                    FileSizeBytes = fileData.Length,
                    Format = request.Format,
                    Type = ExportType.Statistics,
                    RecordCount = 1, // One statistics report
                    DownloadUrl = $"/api/export/download/{Path.GetFileNameWithoutExtension(fileName)}",
                    ProcessingTime = processingTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting statistics for user {UserId}", userId);
                return new ExportResultDto { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<BulkExportResultDto> BulkExportAsync(BulkExportRequestDto request, Guid userId, UserRole userRole, Department userDepartment)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var results = new List<ExportResultDto>();

                foreach (var exportRequest in request.ExportRequests)
                {
                    ExportResultDto result;
                    
                    if (exportRequest.Type == ExportType.Reports)
                    {
                        result = await ExportReportsAsync(exportRequest, userId, userRole, userDepartment);
                    }
                    else if (exportRequest.Type == ExportType.Statistics)
                    {
                        result = await ExportStatisticsAsync(exportRequest, userId, userRole, userDepartment);
                    }
                    else
                    {
                        result = new ExportResultDto { Success = false, ErrorMessage = "Unsupported export type" };
                    }
                    
                    results.Add(result);
                }

                var successfulResults = results.Where(r => r.Success).ToList();
                var totalProcessingTime = DateTime.UtcNow - startTime;

                var bulkResult = new BulkExportResultDto
                {
                    Success = successfulResults.Any(),
                    Results = results,
                    TotalFiles = successfulResults.Count,
                    TotalSizeBytes = successfulResults.Sum(r => r.FileSizeBytes),
                    TotalProcessingTime = totalProcessingTime
                };

                // Create ZIP if requested and there are successful results
                if (request.CreateZipFile && successfulResults.Any())
                {
                    var zipFileName = request.ZipFileName ?? $"Bulk_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
                    // ZIP creation logic would go here
                    bulkResult.ZipFileName = zipFileName;
                    bulkResult.ZipDownloadUrl = $"/api/export/download-zip/{Path.GetFileNameWithoutExtension(zipFileName)}";
                }

                return bulkResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk export for user {UserId}", userId);
                return new BulkExportResultDto { Success = false, ErrorMessage = ex.Message };
            }
        }

        #endregion

        #region Format-Specific Export Methods

        public async Task<byte[]> ExportReportsToPdfAsync(IEnumerable<ReportDetailDto> reports, ExportTemplateDto? template = null)
        {
            return await Task.Run(() =>
            {
                using var stream = new MemoryStream();
                var document = new Document(PageSize.A4, 40, 40, 60, 60);
                var writer = PdfWriter.GetInstance(document, stream);
                
                document.Open();

                // Add header if template provided
                if (template != null && !string.IsNullOrEmpty(template.CompanyName))
                {
                    var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var headerParagraph = new Paragraph(template.CompanyName, headerFont)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    document.Add(headerParagraph);
                    document.Add(new Paragraph("\n"));
                }

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph(template?.ReportTitle ?? "Reports Export", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC"));
                document.Add(new Paragraph($"Total Reports: {reports.Count()}"));
                document.Add(new Paragraph("\n"));

                // Add reports
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                foreach (var report in reports)
                {
                    // Report header
                    var reportTitle = new Paragraph($"Report: {report.Title}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12));
                    document.Add(reportTitle);

                    // Report details table
                    var table = new PdfPTable(2) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 1, 2 });

                    table.AddCell(new PdfPCell(new Phrase("Report Number:", boldFont)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase(report.ReportNumber, bodyFont)) { Border = Rectangle.NO_BORDER });

                    table.AddCell(new PdfPCell(new Phrase("Status:", boldFont)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase(report.Status.ToString(), bodyFont)) { Border = Rectangle.NO_BORDER });

                    table.AddCell(new PdfPCell(new Phrase("Department:", boldFont)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase(report.DepartmentName, bodyFont)) { Border = Rectangle.NO_BORDER });

                    table.AddCell(new PdfPCell(new Phrase("Priority:", boldFont)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase(report.Priority, bodyFont)) { Border = Rectangle.NO_BORDER });

                    table.AddCell(new PdfPCell(new Phrase("Created Date:", boldFont)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase(report.CreatedDate.ToString("yyyy-MM-dd HH:mm"), bodyFont)) { Border = Rectangle.NO_BORDER });

                    table.AddCell(new PdfPCell(new Phrase("Due Date:", boldFont)) { Border = Rectangle.NO_BORDER });
                    table.AddCell(new PdfPCell(new Phrase(report.DueDate?.ToString("yyyy-MM-dd") ?? "Not set", bodyFont)) { Border = Rectangle.NO_BORDER });

                    document.Add(table);

                    // Report content
                    if (!string.IsNullOrEmpty(report.Description))
                    {
                        document.Add(new Paragraph("Description:", boldFont));
                        document.Add(new Paragraph(report.Description, bodyFont));
                    }

                    if (!string.IsNullOrEmpty(report.Content))
                    {
                        document.Add(new Paragraph("Content:", boldFont));
                        // Strip HTML tags for PDF
                        var plainContent = System.Text.RegularExpressions.Regex.Replace(report.Content, "<.*?>", string.Empty);
                        document.Add(new Paragraph(plainContent, bodyFont));
                    }

                    document.Add(new Paragraph("\n"));
                }

                document.Close();
                return stream.ToArray();
            });
        }

        public async Task<byte[]> ExportReportsToExcelAsync(IEnumerable<ReportDetailDto> reports, ExportTemplateDto? template = null)
        {
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Reports Export");

                // Set up headers
                var headers = new string[]
                {
                    "Report Number", "Title", "Status", "Department", "Priority", 
                    "Created Date", "Due Date", "Created By", "Description", "Content"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Add data
                int row = 2;
                foreach (var report in reports)
                {
                    worksheet.Cells[row, 1].Value = report.ReportNumber;
                    worksheet.Cells[row, 2].Value = report.Title;
                    worksheet.Cells[row, 3].Value = report.Status.ToString();
                    worksheet.Cells[row, 4].Value = report.DepartmentName;
                    worksheet.Cells[row, 5].Value = report.Priority;
                    worksheet.Cells[row, 6].Value = report.CreatedDate.ToString("yyyy-MM-dd HH:mm");
                    worksheet.Cells[row, 7].Value = report.DueDate?.ToString("yyyy-MM-dd") ?? "";
                    worksheet.Cells[row, 8].Value = report.CreatorName;
                    worksheet.Cells[row, 9].Value = report.Description ?? "";
                    
                    // Strip HTML from content
                    var plainContent = !string.IsNullOrEmpty(report.Content) 
                        ? System.Text.RegularExpressions.Regex.Replace(report.Content, "<.*?>", string.Empty)
                        : "";
                    worksheet.Cells[row, 10].Value = plainContent;
                    
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add title if template provided
                if (template != null && !string.IsNullOrEmpty(template.ReportTitle))
                {
                    worksheet.InsertRow(1, 2);
                    worksheet.Cells[1, 1].Value = template.ReportTitle;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[2, 1].Value = $"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
                }

                return package.GetAsByteArray();
            });
        }

        public async Task<byte[]> ExportReportsToWordAsync(IEnumerable<ReportDetailDto> reports, ExportTemplateDto? template = null)
        {
            return await Task.Run(() =>
            {
                using var stream = new MemoryStream();
                using var document = OpenXmlDocument.Create(stream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
                
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new OpenXmlWordDocument();
                var body = mainPart.Document.AppendChild(new OpenXmlBody());

                // Add title
                var titleParagraph = body.AppendChild(new OpenXmlParagraph());
                var titleRun = titleParagraph.AppendChild(new OpenXmlRun());
                titleRun.AppendChild(new OpenXmlText(template?.ReportTitle ?? "Reports Export"));

                // Add generation info
                var infoParagraph = body.AppendChild(new OpenXmlParagraph());
                var infoRun = infoParagraph.AppendChild(new OpenXmlRun());
                infoRun.AppendChild(new OpenXmlText($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC"));

                // Add reports
                foreach (var report in reports)
                {
                    // Report title
                    var reportTitleParagraph = body.AppendChild(new OpenXmlParagraph());
                    var reportTitleRun = reportTitleParagraph.AppendChild(new OpenXmlRun());
                    reportTitleRun.AppendChild(new OpenXmlText($"Report: {report.Title}"));

                    // Report details
                    var detailsParagraph = body.AppendChild(new OpenXmlParagraph());
                    var detailsRun = detailsParagraph.AppendChild(new OpenXmlRun());
                    var details = $"Number: {report.ReportNumber}\n" +
                                 $"Status: {report.Status}\n" +
                                 $"Department: {report.DepartmentName}\n" +
                                 $"Priority: {report.Priority}\n" +
                                 $"Created: {report.CreatedDate:yyyy-MM-dd HH:mm}\n" +
                                 $"Due: {report.DueDate?.ToString("yyyy-MM-dd") ?? "Not set"}\n";
                    detailsRun.AppendChild(new OpenXmlText(details));

                    // Description and content
                    if (!string.IsNullOrEmpty(report.Description))
                    {
                        var descParagraph = body.AppendChild(new OpenXmlParagraph());
                        var descRun = descParagraph.AppendChild(new OpenXmlRun());
                        descRun.AppendChild(new OpenXmlText($"Description: {report.Description}"));
                    }

                    if (!string.IsNullOrEmpty(report.Content))
                    {
                        var contentParagraph = body.AppendChild(new OpenXmlParagraph());
                        var contentRun = contentParagraph.AppendChild(new OpenXmlRun());
                        var plainContent = System.Text.RegularExpressions.Regex.Replace(report.Content, "<.*?>", string.Empty);
                        contentRun.AppendChild(new OpenXmlText($"Content: {plainContent}"));
                    }

                    // Add separator
                    var separatorParagraph = body.AppendChild(new OpenXmlParagraph());
                    var separatorRun = separatorParagraph.AppendChild(new OpenXmlRun());
                    separatorRun.AppendChild(new OpenXmlText(""));
                }

                document.Save();
                return stream.ToArray();
            });
        }

        public async Task<byte[]> ExportReportsToCsvAsync(IEnumerable<ReportDetailDto> reports)
        {
            return await Task.Run(() =>
            {
                using var stream = new MemoryStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                
                // Write headers
                writer.WriteLine("Report Number,Title,Status,Department,Priority,Created Date,Due Date,Created By,Description,Content");

                // Write data
                foreach (var report in reports)
                {
                    var line = $"\"{report.ReportNumber}\"," +
                              $"\"{report.Title?.Replace("\"", "\"\"")}\"," +
                              $"\"{report.Status}\"," +
                              $"\"{report.DepartmentName}\"," +
                              $"\"{report.Priority}\"," +
                              $"\"{report.CreatedDate:yyyy-MM-dd HH:mm}\"," +
                              $"\"{report.DueDate?.ToString("yyyy-MM-dd") ?? ""}\"," +
                              $"\"{report.CreatorName?.Replace("\"", "\"\"")}\"," +
                              $"\"{report.Description?.Replace("\"", "\"\"") ?? ""}\"," +
                              $"\"{(string.IsNullOrEmpty(report.Content) ? "" : System.Text.RegularExpressions.Regex.Replace(report.Content, "<.*?>", string.Empty).Replace("\"", "\"\""))}\"";
                    
                    writer.WriteLine(line);
                }

                writer.Flush();
                return stream.ToArray();
            });
        }

        public async Task<byte[]> ExportStatisticsToPdfAsync(ReportStatisticsDto statistics, ExportTemplateDto? template = null)
        {
            return await Task.Run(() =>
            {
                using var stream = new MemoryStream();
                var document = new Document(PageSize.A4, 40, 40, 60, 60);
                var writer = PdfWriter.GetInstance(document, stream);
                
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var title = new Paragraph(template?.ReportTitle ?? "Statistics Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph($"Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC"));
                document.Add(new Paragraph("\n"));

                // Overall Statistics
                var sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                document.Add(new Paragraph("Overall Statistics", sectionFont));
                
                var overallTable = new PdfPTable(2) { WidthPercentage = 100 };
                overallTable.AddCell("Total Reports");
                overallTable.AddCell(statistics.OverallStats.TotalReports.ToString());
                overallTable.AddCell("Total Drafts");
                overallTable.AddCell(statistics.OverallStats.TotalDrafts.ToString());
                overallTable.AddCell("Total Submitted");
                overallTable.AddCell(statistics.OverallStats.TotalSubmitted.ToString());
                overallTable.AddCell("Total Approved");
                overallTable.AddCell(statistics.OverallStats.TotalApproved.ToString());
                overallTable.AddCell("Total Rejected");
                overallTable.AddCell(statistics.OverallStats.TotalRejected.ToString());
                
                document.Add(overallTable);
                document.Add(new Paragraph("\n"));

                // Department Statistics
                if (statistics.DepartmentStats.Any())
                {
                    document.Add(new Paragraph("Department Statistics", sectionFont));
                    
                    var deptTable = new PdfPTable(4) { WidthPercentage = 100 };
                    deptTable.AddCell("Department");
                    deptTable.AddCell("Total Reports");
                    deptTable.AddCell("Approved");
                    deptTable.AddCell("Approval Rate");
                    
                    foreach (var dept in statistics.DepartmentStats)
                    {
                        deptTable.AddCell(dept.DepartmentName);
                        deptTable.AddCell(dept.TotalReports.ToString());
                        deptTable.AddCell(dept.ApprovedReports.ToString());
                        deptTable.AddCell($"{dept.ApprovalRate:F1}%");
                    }
                    
                    document.Add(deptTable);
                }

                document.Close();
                return stream.ToArray();
            });
        }

        public async Task<byte[]> ExportStatisticsToExcelAsync(ReportStatisticsDto statistics, ExportTemplateDto? template = null)
        {
            return await Task.Run(() =>
            {
                using var package = new ExcelPackage();
                
                // Overall Statistics Sheet
                var overallSheet = package.Workbook.Worksheets.Add("Overall Statistics");
                overallSheet.Cells[1, 1].Value = "Metric";
                overallSheet.Cells[1, 2].Value = "Value";
                overallSheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;
                
                int row = 2;
                overallSheet.Cells[row, 1].Value = "Total Reports";
                overallSheet.Cells[row++, 2].Value = statistics.OverallStats.TotalReports;
                overallSheet.Cells[row, 1].Value = "Total Drafts";
                overallSheet.Cells[row++, 2].Value = statistics.OverallStats.TotalDrafts;
                overallSheet.Cells[row, 1].Value = "Total Submitted";
                overallSheet.Cells[row++, 2].Value = statistics.OverallStats.TotalSubmitted;
                overallSheet.Cells[row, 1].Value = "Total Approved";
                overallSheet.Cells[row++, 2].Value = statistics.OverallStats.TotalApproved;
                overallSheet.Cells[row, 1].Value = "Total Rejected";
                overallSheet.Cells[row++, 2].Value = statistics.OverallStats.TotalRejected;
                
                overallSheet.Cells.AutoFitColumns();

                // Department Statistics Sheet
                if (statistics.DepartmentStats.Any())
                {
                    var deptSheet = package.Workbook.Worksheets.Add("Department Statistics");
                    var deptHeaders = new[] { "Department", "Total Reports", "Pending", "Approved", "Rejected", "Approval Rate %" };
                    
                    for (int i = 0; i < deptHeaders.Length; i++)
                    {
                        deptSheet.Cells[1, i + 1].Value = deptHeaders[i];
                        deptSheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }
                    
                    row = 2;
                    foreach (var dept in statistics.DepartmentStats)
                    {
                        deptSheet.Cells[row, 1].Value = dept.DepartmentName;
                        deptSheet.Cells[row, 2].Value = dept.TotalReports;
                        deptSheet.Cells[row, 3].Value = dept.PendingReports;
                        deptSheet.Cells[row, 4].Value = dept.ApprovedReports;
                        deptSheet.Cells[row, 5].Value = dept.RejectedReports;
                        deptSheet.Cells[row, 6].Value = dept.ApprovalRate;
                        row++;
                    }
                    
                    deptSheet.Cells.AutoFitColumns();
                }

                return package.GetAsByteArray();
            });
        }

        #endregion

        #region Template and Utility Methods

        public async Task<IEnumerable<ExportTemplateDto>> GetExportTemplatesAsync()
        {
            // Mock implementation - in real scenario, this would be from database
            return await Task.FromResult(new List<ExportTemplateDto>
            {
                new ExportTemplateDto
                {
                    CompanyName = "Project Controls Reporting Tool",
                    ReportTitle = "Standard Report Export",
                    IncludeCoverPage = true,
                    IncludePageNumbers = true,
                    PageOrientation = "Portrait",
                    PageSize = "A4"
                }
            });
        }

        public async Task<ExportTemplateDto?> GetExportTemplateAsync(string templateName)
        {
            var templates = await GetExportTemplatesAsync();
            return templates.FirstOrDefault(t => t.ReportTitle == templateName);
        }

        public async Task<ExportTemplateDto> CreateExportTemplateAsync(ExportTemplateDto template, Guid userId)
        {
            // Mock implementation - would save to database
            return await Task.FromResult(template);
        }

        public async Task<bool> DeleteExportTemplateAsync(string templateName, Guid userId)
        {
            // Mock implementation - would delete from database
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<ExportHistoryDto>> GetExportHistoryAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            // Mock implementation - would get from database
            return await Task.FromResult(new List<ExportHistoryDto>());
        }

        public async Task<ExportResultDto?> GetExportResultAsync(Guid exportId, Guid userId)
        {
            // Mock implementation - would get from database
            return await Task.FromResult<ExportResultDto?>(null);
        }

        public async Task<byte[]?> DownloadExportAsync(Guid exportId, Guid userId)
        {
            // Mock implementation - would get file from storage
            return await Task.FromResult<byte[]?>(null);
        }

        public async Task<bool> DeleteExportAsync(Guid exportId, Guid userId)
        {
            // Mock implementation - would delete from storage and database
            return await Task.FromResult(true);
        }

        public async Task<bool> ValidateExportRequestAsync(ExportRequestDto request, Guid userId, UserRole userRole)
        {
            // Basic validation
            if (request.Format == ExportFormat.PDF || request.Format == ExportFormat.Excel ||
                request.Format == ExportFormat.Word || request.Format == ExportFormat.CSV)
            {
                return await Task.FromResult(true);
            }
            
            return await Task.FromResult(false);
        }

        public async Task<string> GetContentTypeForFormat(ExportFormat format)
        {
            return await Task.FromResult(format switch
            {
                ExportFormat.PDF => "application/pdf",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.Word => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ExportFormat.CSV => "text/csv",
                ExportFormat.JSON => "application/json",
                ExportFormat.XML => "application/xml",
                _ => "application/octet-stream"
            });
        }

        public async Task<string> GetFileExtensionForFormat(ExportFormat format)
        {
            return await Task.FromResult(format switch
            {
                ExportFormat.PDF => ".pdf",
                ExportFormat.Excel => ".xlsx",
                ExportFormat.Word => ".docx",
                ExportFormat.CSV => ".csv",
                ExportFormat.JSON => ".json",
                ExportFormat.XML => ".xml",
                _ => ".bin"
            });
        }

        #endregion
    }
}
