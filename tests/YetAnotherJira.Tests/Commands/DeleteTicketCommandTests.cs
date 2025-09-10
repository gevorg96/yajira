using YetAnotherJira.Application.Commands;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Tests.Commands;

public class DeleteTicketCommandTests : TestBase
{
    [Fact]
    public async Task Handle_ExistingTicket_ShouldMarkAsDeleted()
    {
        ClearChangeTracker();  
        var handler = new DeleteTicketCommandHandler(DbContext, GetLogger<DeleteTicketCommandHandler>());
        var command = new DeleteTicketCommand(Id: 1);
        
        var ticketBefore = await DbContext.Tickets.FindAsync(1L);
        ticketBefore.Should().NotBeNull();
        ticketBefore!.IsDeleted.Should().BeFalse();
        
        ClearChangeTracker();  
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var ticketAfter = await DbContext.Tickets.FindAsync(1L);
        ticketAfter.Should().NotBeNull();
        ticketAfter!.IsDeleted.Should().BeTrue();
        ticketAfter.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ShouldThrowTicketNotFoundException()
    {
        var handler = new DeleteTicketCommandHandler(DbContext, GetLogger<DeleteTicketCommandHandler>());
        var command = new DeleteTicketCommand(Id: 999);
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task Handle_AlreadyDeletedTicket_ShouldStillWork()
    {
        ClearChangeTracker(); 
        
        var ticket = await DbContext.Tickets.FindAsync(1L);
        ticket!.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        ClearChangeTracker(); 
        var handler = new DeleteTicketCommandHandler(DbContext, GetLogger<DeleteTicketCommandHandler>());
        var command = new DeleteTicketCommand(Id: 1);
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var ticketAfter = await DbContext.Tickets.FindAsync(1L);
        ticketAfter!.IsDeleted.Should().BeTrue();
    }
}
