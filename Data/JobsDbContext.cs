using Microsoft.EntityFrameworkCore;

namespace ImplementUserFacingLogFiles.Data;

public class JobsDbContext : DbContext
{
    public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; }
}