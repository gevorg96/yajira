using YetAnotherJira.Application.Commands;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Tests.Commands;

public class UpdateTicketFieldCommandTests : TestBase
{
    [Fact]
    public async Task UpdateTicketTitleCommand_ValidRequest_ShouldUpdateTitle()
    {
        ClearChangeTracker();  
        var handler = new UpdateTicketTitleCommandHandler(DbContext, GetLogger<UpdateTicketTitleCommandHandler>());
        var command = new UpdateTicketTitleCommand(Id: 1, Title: "Updated Title");
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.Title.Should().Be("Updated Title");
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTicketTitleCommand_NonExistentTicket_ShouldThrowTicketNotFoundException()
    {
        var handler = new UpdateTicketTitleCommandHandler(DbContext, GetLogger<UpdateTicketTitleCommandHandler>());
        var command = new UpdateTicketTitleCommand(Id: 999, Title: "New Title");
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task UpdateTicketDescriptionCommand_ValidRequest_ShouldUpdateDescription()
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketDescriptionCommandHandler(DbContext, GetLogger<UpdateTicketDescriptionCommandHandler>());
        var command = new UpdateTicketDescriptionCommand(Id: 1, Description: "Updated Description");
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.Description.Should().Be("Updated Description");
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTicketAuthorCommand_ValidRequest_ShouldUpdateAuthor()
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketAuthorCommandHandler(DbContext, GetLogger<UpdateTicketAuthorCommandHandler>());
        var command = new UpdateTicketAuthorCommand(Id: 1, Author: "new_author");
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.Author.Should().Be("new_author");
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTicketExecutorCommand_ValidRequest_ShouldUpdateExecutor()
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketAssigneeCommandHandler(DbContext, GetLogger<UpdateTicketAssigneeCommandHandler>());
        var command = new UpdateTicketAssigneeCommand(Id: 1, Assignee: "new_executor");
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.Assignee.Should().Be("new_executor");
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(TicketStatus.New)]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.Done)]
    public async Task UpdateTicketStatusCommand_DifferentStatuses_ShouldUpdateStatus(TicketStatus status)
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketStatusCommandHandler(DbContext, GetLogger<UpdateTicketStatusCommandHandler>());
        var command = new UpdateTicketStatusCommand(Id: 1, Status: status);
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.Status.Should().Be(status);
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(TicketPriority.Low)]
    [InlineData(TicketPriority.Medium)]
    [InlineData(TicketPriority.High)]
    public async Task UpdateTicketPriorityCommand_DifferentPriorities_ShouldUpdatePriority(TicketPriority priority)
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketPriorityCommandHandler(DbContext, GetLogger<UpdateTicketPriorityCommandHandler>());
        var command = new UpdateTicketPriorityCommand(Id: 1, Priority: priority);
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.Priority.Should().Be(priority);
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTicketParentCommand_SetValidParent_ShouldUpdateParent()
    {
        ClearChangeTracker(); 
        var handler = new UpdateTicketParentCommandHandler(DbContext, GetLogger<UpdateTicketParentCommandHandler>());
        var command = new UpdateTicketParentCommand(Id: 1, Parent: 2);
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.ParentTaskId.Should().Be(2);
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTicketParentCommand_RemoveParent_ShouldSetParentToNull()
    {
        ClearChangeTracker(); 
        
        var ticket = await DbContext.Tickets.FindAsync(1L);
        ticket!.ParentTaskId = 2;
        await DbContext.SaveChangesAsync();

        ClearChangeTracker(); 
        var handler = new UpdateTicketParentCommandHandler(DbContext, GetLogger<UpdateTicketParentCommandHandler>());
        var command = new UpdateTicketParentCommand(Id: 1, Parent: null);
        
        await handler.Handle(command, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await DbContext.Tickets.FindAsync(1L);
        updatedTicket!.ParentTaskId.Should().BeNull();
        updatedTicket.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateTicketParentCommand_InvalidParent_ShouldThrowTicketNotFoundException()
    {
        var handler = new UpdateTicketParentCommandHandler(DbContext, GetLogger<UpdateTicketParentCommandHandler>());
        var command = new UpdateTicketParentCommand(Id: 1, Parent: 999);
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Theory]
    [InlineData("UpdateTicketDescriptionCommand")]
    [InlineData("UpdateTicketAuthorCommand")]
    [InlineData("UpdateTicketExecutorCommand")]
    public async Task UpdateFieldCommands_NonExistentTicket_ShouldThrowTicketNotFoundException(string commandType)
    {
        var exception = commandType switch
        {
            "UpdateTicketDescriptionCommand" => await Assert.ThrowsAsync<TicketNotFoundException>(async () =>
            {
                var handler = new UpdateTicketDescriptionCommandHandler(DbContext, GetLogger<UpdateTicketDescriptionCommandHandler>());
                var command = new UpdateTicketDescriptionCommand(Id: 999, Description: "New Description");
                await handler.Handle(command, CancellationToken.None);
            }),
            "UpdateTicketAuthorCommand" => await Assert.ThrowsAsync<TicketNotFoundException>(async () =>
            {
                var handler = new UpdateTicketAuthorCommandHandler(DbContext, GetLogger<UpdateTicketAuthorCommandHandler>());
                var command = new UpdateTicketAuthorCommand(Id: 999, Author: "new_author");
                await handler.Handle(command, CancellationToken.None);
            }),
            "UpdateTicketExecutorCommand" => await Assert.ThrowsAsync<TicketNotFoundException>(async () =>
            {
                var handler = new UpdateTicketAssigneeCommandHandler(DbContext, GetLogger<UpdateTicketAssigneeCommandHandler>());
                var command = new UpdateTicketAssigneeCommand(Id: 999, Assignee: "new_executor");
                await handler.Handle(command, CancellationToken.None);
            }),
            _ => throw new ArgumentException("Invalid command type")
        };

        exception.Message.Should().Contain("999");
    }
}
