using ImplementUserFacingLogFiles.Data;
using ImplementUserFacingLogFiles.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ImplementUserFacingLogFiles.Pages;

public class IndexModel(JobsDbContext dbContext, JobLoggerFactory jobLoggerFactory, ILogger<IndexModel> logger) : PageModel
{
    public List<Job>? Jobs { get; set; }

    public async Task OnGet()
    {
        Jobs = await dbContext.Jobs
            .OrderByDescending(j => j.QueuedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostNormal()
    {
        logger.LogInformation("Start executing the normal action");

        await CreateSampleJobWithLogEntries();

        logger.LogInformation("Finished executing the normal action");
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostWithWarnings()
    {
        logger.LogInformation("Start executing the with warnings action");

        await CreateSampleJobWithLogEntries(withWarnings: true);

        logger.LogInformation("Finished executing the with warnings action");
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostWithErrors()
    {
        logger.LogInformation("Start executing the with errors action");

        await CreateSampleJobWithLogEntries(withErrors: true);

        logger.LogInformation("Finished executing the with errors action");
        return RedirectToPage("Index");
    }

    private async Task CreateSampleJobWithLogEntries(bool withWarnings = false, bool withErrors = false)
    {
        var jobId = Guid.NewGuid();

        var monitoringLogger = jobLoggerFactory.CreateLogger(jobId);
        var jobLogger = monitoringLogger as Serilog.ILogger;
        jobLogger.Information("Start exporting document \"Emma Brooks\"");
        jobLogger.Information("Exporting content to \"Test Space: RichTextTest\" (\"CONTENTFUL\")");
        jobLogger.Information("Setting field values");
        jobLogger.Information("Setting value for \"title\"");
        jobLogger.Information("Setting value for \"body\"");
        jobLogger.Information("Setting value for \"byline\"");
        if (withWarnings)
        {
            jobLogger.Warning("Field \"slug\" was not found");
        }

        if (withErrors)
        {
            jobLogger.Error("Could not write document content to \"Test Space: RichTextTest\" (\"CONTENTFUL\")");
        }

        jobLogger.Information("Finished exporting document \"Emma Brooks\"");

        var job = new Job
        {
            Id = jobId,
            QueuedAt = DateTime.Now,
            HasWarnings = monitoringLogger.HasWarnings,
            HasErrors = monitoringLogger.HasErrors,
        };
        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync();
    }
}