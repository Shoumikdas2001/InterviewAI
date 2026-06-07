using InterviewAI.Application.Interfaces;
using InterviewAI.Domain.Entities;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InterviewAI.Infrastructure.ExternalServices;

public class ReportService : IReportService
{
    private readonly IInterviewSessionRepository _sessionRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly IQuestionRepository _questionRepo;
    private readonly IReportRepository _reportRepo;
    private readonly IUserRepository _userRepo;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IInterviewSessionRepository sessionRepo,
        IAnswerRepository answerRepo,
        IQuestionRepository questionRepo,
        IReportRepository reportRepo,
        IUserRepository userRepo,
        ILogger<ReportService> logger)
    {
        _sessionRepo = sessionRepo;
        _answerRepo = answerRepo;
        _questionRepo = questionRepo;
        _reportRepo = reportRepo;
        _userRepo = userRepo;
        _logger = logger;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateReportAsync(string sessionId, string userId, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId, ct)
            ?? throw new InvalidOperationException("Session not found.");
        if (session.UserId != userId) throw new UnauthorizedAccessException();

        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        var questions = (await _questionRepo.GetBySessionIdAsync(sessionId, ct)).ToList();
        var answers = (await _answerRepo.GetBySessionIdAsync(sessionId, ct)).ToList();

        // Build Q&A pairs for the report
        var qaPairs = questions.Select(q => new
        {
            Question = q,
            Answer = answers.FirstOrDefault(a => a.QuestionId == q.Id)
        }).ToList();

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial));
                page.Background().Background("#FAFAFA");

                // Header
                page.Header().Element(ComposeHeader(session, user));

                // Content
                page.Content().Element(content =>
                {
                    content.PaddingVertical(10).Column(col =>
                    {
                        // Overall Score Section
                        col.Item().Element(ComposeScoreSection(session));
                        col.Item().PaddingTop(20).Element(ComposeSkillGapSection(session));

                        if (qaPairs.Any())
                        {
                            col.Item().PaddingTop(20).Element(ComposeQASection(qaPairs
                                .Select(qa => (qa.Question, qa.Answer))
                                .ToList()));
                        }

                        col.Item().PaddingTop(20).Element(ComposeRecommendationsSection(session));
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("InterviewAI Report • Generated on ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.UtcNow.ToString("MMMM d, yyyy")).FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" • Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();

        // Save report to DB
        var report = new Report
        {
            UserId = userId,
            SessionId = sessionId,
            Title = $"Interview Report — {session.JobRole}",
            JobRole = session.JobRole,
            OverallScore = session.OverallScore ?? 0,
            Strengths = session.SkillGapAnalysis?.Strengths ?? [],
            Weaknesses = session.SkillGapAnalysis?.Weaknesses ?? [],
            TopicsCovered = questions.Select(q => q.SkillTag).Distinct().ToList(),
            RecommendedTopics = session.SkillGapAnalysis?.MissingTopics ?? [],
            GeneralFeedback = $"Overall performance: {session.OverallScore:F1}/10",
            PdfData = pdfBytes
        };

        await _reportRepo.CreateAsync(report, ct);

        // Update session with report ID
        session.ReportId = report.Id;
        await _sessionRepo.UpdateAsync(session.Id, session, ct);

        return pdfBytes;
    }

    public async Task<byte[]> GetReportAsync(string reportId, string userId, CancellationToken ct = default)
    {
        var report = await _reportRepo.GetByIdAsync(reportId, ct)
            ?? throw new InvalidOperationException("Report not found.");
        if (report.UserId != userId) throw new UnauthorizedAccessException();
        return report.PdfData ?? throw new InvalidOperationException("Report file not found.");
    }

    private static Action<IContainer> ComposeHeader(InterviewSession session, User user) =>
        container =>
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("InterviewAI").FontSize(24).Bold().FontColor("#6C63FF");
                    col.Item().Text("Interview Performance Report").FontSize(14).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(4).Text($"Candidate: {user.FirstName} {user.LastName}").FontSize(11);
                    col.Item().Text($"Role: {session.JobRole}").FontSize(11);
                    col.Item().Text($"Date: {session.CreatedAt:MMMM d, yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(100).AlignRight().AlignMiddle().Column(col =>
                {
                    var score = session.OverallScore ?? 0;
                    var scoreColor = score >= 7 ? "#10B981" : score >= 5 ? "#F59E0B" : "#EF4444";
                    col.Item().Text($"{score:F1}").FontSize(36).Bold().FontColor(scoreColor).AlignCenter();
                    col.Item().Text("/10").FontSize(12).FontColor(Colors.Grey.Medium).AlignCenter();
                    col.Item().Text("Overall Score").FontSize(9).FontColor(Colors.Grey.Medium).AlignCenter();
                });
            });

            container.PaddingTop(8).LineHorizontal(2).LineColor("#6C63FF");
        };

    private static Action<IContainer> ComposeScoreSection(InterviewSession session) =>
        container =>
        {
            container.Column(col =>
            {
                col.Item().Text("Performance Breakdown").FontSize(14).Bold().FontColor("#1F2937");
                col.Item().PaddingTop(8).Row(row =>
                {
                    ScoreCard(row, "Technical", session.TechnicalScore ?? 0);
                    ScoreCard(row, "Communication", session.CommunicationScore ?? 0);
                    ScoreCard(row, "Confidence", session.ConfidenceScore ?? 0);
                    ScoreCard(row, "Problem Solving", session.ProblemSolvingScore ?? 0);
                });
            });
        };

    private static void ScoreCard(RowDescriptor row, string label, double score)
    {
        var color = score >= 7 ? "#10B981" : score >= 5 ? "#F59E0B" : "#EF4444";
        row.RelativeItem().Border(1).BorderColor("#E5E7EB").Padding(10).Column(c =>
        {
            c.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Medium).AlignCenter();
            c.Item().Text($"{score:F1}").FontSize(20).Bold().FontColor(color).AlignCenter();
        });
    }

    private static Action<IContainer> ComposeSkillGapSection(InterviewSession session) =>
        container =>
        {
            if (session.SkillGapAnalysis is null) return;
            var gap = session.SkillGapAnalysis;

            container.Column(col =>
            {
                col.Item().Text("Skill Gap Analysis").FontSize(14).Bold().FontColor("#1F2937");

                col.Item().PaddingTop(8).Row(row =>
                {
                    // Strengths
                    row.RelativeItem().Border(1).BorderColor("#D1FAE5").Background("#F0FDF4").Padding(10).Column(c =>
                    {
                        c.Item().Text("✓ Strengths").FontSize(11).Bold().FontColor("#065F46");
                        foreach (var s in gap.Strengths.Take(4))
                            c.Item().PaddingTop(3).Text($"• {s}").FontSize(9).FontColor("#374151");
                    });

                    row.ConstantItem(10);

                    // Weaknesses
                    row.RelativeItem().Border(1).BorderColor("#FEE2E2").Background("#FFF5F5").Padding(10).Column(c =>
                    {
                        c.Item().Text("✗ Weaknesses").FontSize(11).Bold().FontColor("#991B1B");
                        foreach (var w in gap.Weaknesses.Take(4))
                            c.Item().PaddingTop(3).Text($"• {w}").FontSize(9).FontColor("#374151");
                    });
                });
            });
        };

    private static Action<IContainer> ComposeQASection(
        List<(Question Question, Answer? Answer)> qaPairs) =>
        container =>
        {
            container.Column(col =>
            {
                col.Item().Text("Interview Questions & Feedback").FontSize(14).Bold().FontColor("#1F2937");
                col.Item().PaddingTop(4).Text("Top 5 Q&A pairs with AI evaluation").FontSize(9).FontColor(Colors.Grey.Medium);

                foreach (var (q, a) in qaPairs.Take(5))
                {
                    col.Item().PaddingTop(10).Border(1).BorderColor("#E5E7EB").Padding(10).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Q: {q.QuestionText}").FontSize(10).Bold();
                            if (a?.Evaluation is not null)
                            {
                                var sc = a.Evaluation.OverallScore;
                                var sc_color = sc >= 7 ? "#10B981" : sc >= 5 ? "#F59E0B" : "#EF4444";
                                r.ConstantItem(40).AlignRight()
                                    .Text($"{sc}/10").FontSize(11).Bold().FontColor(sc_color);
                            }
                        });

                        if (!string.IsNullOrEmpty(a?.AnswerText))
                        {
                            var truncated = a.AnswerText.Length > 200
                                ? a.AnswerText[..200] + "..."
                                : a.AnswerText;
                            c.Item().PaddingTop(4).Text($"A: {truncated}").FontSize(9).FontColor("#6B7280");
                        }

                        if (a?.Evaluation?.Feedback.Any() == true)
                        {
                            c.Item().PaddingTop(4).Text("Feedback:").FontSize(9).Bold().FontColor("#6C63FF");
                            c.Item().Text(a.Evaluation.Feedback.First()).FontSize(9).FontColor("#374151").Italic();
                        }
                    });
                }
            });
        };

    private static Action<IContainer> ComposeRecommendationsSection(InterviewSession session) =>
        container =>
        {
            if (session.SkillGapAnalysis?.Recommendations.Any() != true) return;

            container.Column(col =>
            {
                col.Item().Text("Recommendations").FontSize(14).Bold().FontColor("#1F2937");
                col.Item().PaddingTop(8).Border(1).BorderColor("#EDE9FE").Background("#F5F3FF").Padding(12).Column(c =>
                {
                    foreach (var rec in session.SkillGapAnalysis!.Recommendations.Take(6))
                    {
                        c.Item().PaddingBottom(4).Row(r =>
                        {
                            r.ConstantItem(16).Text("→").FontColor("#6C63FF").FontSize(10);
                            r.RelativeItem().Text(rec).FontSize(10).FontColor("#374151");
                        });
                    }
                });
            });
        };
}
