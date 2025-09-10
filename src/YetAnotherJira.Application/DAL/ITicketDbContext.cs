using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace YetAnotherJira.Application.DAL;

public interface ITicketDbContext
{
    DbSet<TicketDal> Tickets { get; set; }

    DbSet<TicketRelationDal> TicketRelations { get; set; }
    
    DbSet<UserDal> Users { get; set; }

    EntityEntry Entry(object entity);
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    Task<TicketDal> SelectForUpdate(long id, CancellationToken token);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}