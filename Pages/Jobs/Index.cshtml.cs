using System.Globalization;
using System.Text.RegularExpressions;
using ImplementUserFacingLogFiles.Data;
using ImplementUserFacingLogFiles.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ImplementUserFacingLogFiles.Pages.Jobs;

public class IndexModel(JobsDbContext dbContext, JobLoggerFactory jobLoggerFactory) : PageModel
{
    public JobDetailModel? JobDetail { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        var job = await dbContext.Jobs
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
        {
            return NotFound();
        }

        JobDetail = new JobDetailModel(id, await GetLogEntries(id));
        
        return Page();
    }

    private async Task<List<JobDetailModel.LogEntryModel>> GetLogEntries(Guid jobId)
    {
        var logEntries = new List<JobDetailModel.LogEntryModel>();
        var logContent = await jobLoggerFactory.GetLogContents(jobId);
        var logLines = logContent.Split(Environment.NewLine);
        
        string? previousLevel = null;
        foreach (var logLine in logLines)
        {
            if (!string.IsNullOrEmpty(logLine))
            {
                var regex = new Regex(@"^\[(?<date>\d{4}-\d{2}-\d{2} \d{2}\:\d{2}\:\d{2}\.\d{3}\ (\+|\-)\d{2}\:\d{2}) (?<level>[A-Z]{3})\] (?<text>.+)$");
                var match = regex.Match(logLine);
                if (match.Success)
                {
                    var date = match.Groups["date"].Value.Trim();
                    var level = match.Groups["level"].Value;
                    var text = match.Groups["text"].Value;

                    previousLevel = level;

                    if (!string.IsNullOrEmpty(date)
                        && DateTimeOffset.TryParseExact(date, "yyyy-MM-dd HH:mm:ss.fff zzz", null, DateTimeStyles.AssumeUniversal, out var dateTimeOffset))
                    {
                        logEntries.Add(new JobDetailModel.LogEntryModel(dateTimeOffset, level, text));
                    }
                    else
                    {
                        logEntries.Add(new JobDetailModel.LogEntryModel(null, level, text));
                    }
                }
                else
                {
                    logEntries.Add(new JobDetailModel.LogEntryModel(null, previousLevel, logLine));
                }
            }
        }

        return logEntries;
    }

    public record JobDetailModel(Guid Id, List<JobDetailModel.LogEntryModel> LogEntries)
    {
        public record LogEntryModel(DateTimeOffset? Time, string? Level, string Text);
    }
}