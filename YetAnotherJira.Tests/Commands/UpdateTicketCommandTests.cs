using YetAnotherJira.Application.Commands;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Tests.Commands;

public class UpdateTicketCommandTests : TestBase
{
    [Fact]
    public async Task Handle_UpdateAllFields_ShouldUpdateTicketCorrectly()
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        var command = new UpdateTicketCommand(
            Id: 1,
            Title: "Updated Title",
            Description: "Updated Description",
            Author: "updated_author",
            Assignee: "updated_assignee",
            Parent: 2
        );
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket.Should().NotBeNull();
        updatedTicket!.Title.Should().Be("Updated Title");
        updatedTicket.Description.Should().Be("Updated Description");
        updatedTicket.Author.Should().Be("updated_author");
        updatedTicket.Assignee.Should().Be("updated_assignee");
        updatedTicket.ParentTaskId.Should().Be(2);
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_PartialUpdate_ShouldUpdateOnlySpecifiedFields()
    {
        ClearChangeTracker(); 
        var originalTicket = await DbContext.Tickets.FindAsync(1L);
        var originalTitle = originalTicket!.Title;
        var originalDescription = originalTicket.Description;
        
        ClearChangeTracker(); 
        var handler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        var command = new UpdateTicketCommand(
            Id: 1,
            Title: null, 
            Description: null, 
            Author: "new_author", 
            Assignee: null, 
            Parent: null 
        );
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket.Should().NotBeNull();
        updatedTicket!.Title.Should().Be(originalTitle); 
        updatedTicket.Description.Should().Be(originalDescription); 
        updatedTicket.Author.Should().Be("new_author"); 
        updatedTicket.ParentTaskId.Should().BeNull(); 
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ShouldThrowTicketNotFoundException()
    {
        var handler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        var command = new UpdateTicketCommand(
            Id: 999,
            Title: "Updated Title",
            Description: null,
            Author: null,
            Assignee: null,
            Parent: null
        );
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task Handle_InvalidParent_ShouldThrowTicketNotFoundException()
    {
        var handler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        var command = new UpdateTicketCommand(
            Id: 1,
            Title: null,
            Description: null,
            Author: null,
            Assignee: null,
            Parent: 999 
        );
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task Handle_RemoveParent_ShouldSetParentToNull()
    {
        ClearChangeTracker(); 
        var ticket = await DbContext.Tickets.FindAsync(1L);
        ticket!.ParentTaskId = 2;
        await DbContext.SaveChangesAsync();

        ClearChangeTracker(); 
        var handler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        var command = new UpdateTicketCommand(
            Id: 1,
            Title: null,
            Description: null,
            Author: null,
            Assignee: null,
            Parent: null 
        );
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.ParentTaskId.Should().BeNull();
    }
}
