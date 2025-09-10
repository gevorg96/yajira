using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using YetAnotherJira.Application.DAL;

namespace YetAnotherJira.Infrastructure;

public class TicketDbContext(DbContextOptions<TicketDbContext> options) : DbContext(options), ITicketDbContext
{
    public DbSet<TicketDal> Tickets { get; set; }
    
    public DbSet<TicketRelationDal> TicketRelations { get; set; }
    
    public DbSet<UserDal> Users { get; set; }
    
    public Task<TicketDal> SelectForUpdate(long id, CancellationToken token)
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // For in-memory database, use regular query with tracking disabled
            return Tickets.Where(t => t.Id == id).AsNoTracking().FirstOrDefaultAsync(token);
        }
        
        // For PostgreSQL, use FOR UPDATE with tracking disabled
        return Tickets.FromSqlRaw("SELECT * FROM tickets where id = {0} FOR UPDATE", id).AsNoTracking().FirstOrDefaultAsync(token);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketDal>()
            .HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TicketRelationDal>()
            .HasKey(tr => new { tr.FromTaskId, tr.ToTaskId, tr.RelationType });

        modelBuilder.Entity<TicketRelationDal>()
            .HasOne(tr => tr.FromTask)
            .WithMany(t => t.OutgoingRelations)
            .HasForeignKey(tr => tr.FromTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketRelationDal>()
            .HasOne(tr => tr.ToTask)
            .WithMany(t => t.IncomingRelations)
            .HasForeignKey(tr => tr.ToTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TicketRelationDal>()
            .ToTable(t => t.HasCheckConstraint("CK_TaskRelation_NoSelfReference", "from_task_id != to_task_id"));
        
        modelBuilder.Entity<TicketRelationDal>()
            .HasIndex(tr => tr.FromTaskId);
        
        modelBuilder.Entity<TicketRelationDal>()
            .HasIndex(tr => tr.ToTaskId);
    }
}