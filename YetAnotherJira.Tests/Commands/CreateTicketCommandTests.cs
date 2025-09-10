using YetAnotherJira.Application.Commands;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Tests.Commands;

public class CreateTicketCommandTests : TestBase
{
    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateTicket()
    {
        var handler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var command = new CreateTicketCommand(
            Priority: TicketPriority.High,
            Title: "New Test Ticket",
            Description: "New Test Description",
            Author: "admin",
            Assignee: "user1",
            Parent: null
        );
        
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.Should().BeGreaterThan(0);
        
        var createdTicket = await DbContext.Tickets.FindAsync(result);
        createdTicket.Should().NotBeNull();
        createdTicket!.Title.Should().Be("New Test Ticket");
        createdTicket.Description.Should().Be("New Test Description");
        createdTicket.Author.Should().Be("admin");
        createdTicket.Assignee.Should().Be("user1");
        createdTicket.Priority.Should().Be(TicketPriority.High);
        createdTicket.Status.Should().Be(TicketStatus.New);
        createdTicket.IsDeleted.Should().BeFalse();
        createdTicket.ParentTaskId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithValidParent_ShouldCreateTicketWithParent()
    {
         
        var handler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var command = new CreateTicketCommand(
            Priority: TicketPriority.Medium,
            Title: "Child Ticket",
            Description: "Child Description",
            Author: "admin",
            Assignee: "user1",
            Parent: 1 
        );
        
        var result = await handler.Handle(command, CancellationToken.None);
        
        result.Should().BeGreaterThan(0);
        
        var createdTicket = await DbContext.Tickets.FindAsync(result);
        createdTicket.Should().NotBeNull();
        createdTicket!.ParentTaskId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithInvalidParent_ShouldThrowTicketNotFoundException()
    {
        var handler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var command = new CreateTicketCommand(
            Priority: TicketPriority.Low,
            Title: "Invalid Parent Ticket",
            Description: "Description",
            Author: "admin",
            Assignee: "user1",
            Parent: 999 
        );
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Theory]
    [InlineData(TicketPriority.Low)]
    [InlineData(TicketPriority.Medium)]
    [InlineData(TicketPriority.High)]
    public async Task Handle_DifferentPriorities_ShouldCreateTicketWithCorrectPriority(TicketPriority priority)
    {
        var handler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var command = new CreateTicketCommand(
            Priority: priority,
            Title: $"Priority {priority} Ticket",
            Description: "Test Description",
            Author: "admin",
            Assignee: "user1",
            Parent: null
        );
        
        var result = await handler.Handle(command, CancellationToken.None);
        
        var createdTicket = await DbContext.Tickets.FindAsync(result);
        createdTicket!.Priority.Should().Be(priority);
    }
}
