using YetAnotherJira.Application.Commands;
using YetAnotherJira.Application.Queries;
using YetAnotherJira.Domain.Enums;

namespace YetAnotherJira.Tests.Integration;

public class FullWorkflowIntegrationTests : TestBase
{
    [Fact]
    public async Task CompleteTicketWorkflow_ShouldWorkEndToEnd()
    {
        var createHandler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var updateHandler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        var getHandler = new GetTicketQueryHandler(DbContext);
        var deleteHandler = new DeleteTicketCommandHandler(DbContext, GetLogger<DeleteTicketCommandHandler>());
         
        var createCommand = new CreateTicketCommand(
            Priority: TicketPriority.High,
            Title: "Integration Test Ticket",
            Description: "Testing full workflow",
            Author: "admin",
            Assignee: "user1",
            Parent: null
        );

        var ticketId = await createHandler.Handle(createCommand, CancellationToken.None);
        ticketId.Should().BeGreaterThan(0);
        
        var getQuery = new GetTicketQuery(ticketId);
        var ticket = await getHandler.Handle(getQuery, CancellationToken.None);
        
        ticket.Title.Should().Be("Integration Test Ticket");
        ticket.Status.Should().Be(TicketStatus.New);
        ticket.Priority.Should().Be(TicketPriority.High);
        
        var updateCommand = new UpdateTicketCommand(
            Id: ticketId,
            Title: "Updated Integration Test Ticket",
            Description: "Updated description",
            Author: null,
            Assignee: "new_assignee",
            Parent: null
        );

        ClearChangeTracker(); 
        await updateHandler.Handle(updateCommand, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await getHandler.Handle(getQuery, CancellationToken.None);
        updatedTicket.Title.Should().Be("Updated Integration Test Ticket");
        updatedTicket.Description.Should().Be("Updated description");
        updatedTicket.Assignee.Should().Be("new_assignee");
        updatedTicket.Author.Should().Be("admin"); 
        
        var deleteCommand = new DeleteTicketCommand(ticketId);
        await deleteHandler.Handle(deleteCommand, CancellationToken.None);
        
        var deletedTicket = await getHandler.Handle(getQuery, CancellationToken.None);
        deletedTicket.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task TicketRelationWorkflow_ShouldWorkEndToEnd()
    {
        var addRelationHandler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var deleteRelationHandler = new DeleteTicketRelatesToCommandHandler(DbContext, GetLogger<DeleteTicketRelatesToCommandHandler>());
        var getHandler = new GetTicketQueryHandler(DbContext);
        
        var addRelationCommand = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 2,
            RelationType: TicketRelationType.Blocks
        );

        await addRelationHandler.Handle(addRelationCommand, CancellationToken.None);
        
        var ticket1 = await getHandler.Handle(new GetTicketQuery(1), CancellationToken.None);
        ticket1.RelatedTickets.Should().HaveCount(1);
        ticket1.RelatedTickets.First().RelationType.Should().Be(TicketRelationType.Blocks);
        ticket1.RelatedTickets.First().RelatedTicket.Id.Should().Be(2);
        
        var addRelationCommand2 = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 2,
            RelationType: TicketRelationType.Blocks 
        );
        
        await Assert.ThrowsAsync<YetAnotherJira.Domain.Exceptions.RelationAlreadyExistsException>(
            () => addRelationHandler.Handle(addRelationCommand2, CancellationToken.None));

        
        var deleteRelationCommand = new DeleteTicketRelatesToCommand(
            Id: 1,
            RelatesTo: new[] { 2L }
        );

        await deleteRelationHandler.Handle(deleteRelationCommand, CancellationToken.None);
        
        var updatedTicket1 = await getHandler.Handle(new GetTicketQuery(1), CancellationToken.None);
        updatedTicket1.RelatedTickets.Should().BeEmpty();
    }

    [Fact]
    public async Task ParentChildTicketWorkflow_ShouldWorkEndToEnd()
    {
        var createHandler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var updateParentHandler = new UpdateTicketParentCommandHandler(DbContext, GetLogger<UpdateTicketParentCommandHandler>());
        var getHandler = new GetTicketQueryHandler(DbContext);
        
        var createChildCommand = new CreateTicketCommand(
            Priority: TicketPriority.Medium,
            Title: "Child Ticket",
            Description: "Child ticket description",
            Author: "admin",
            Assignee: "user1",
            Parent: 1 
        );

        var childTicketId = await createHandler.Handle(createChildCommand, CancellationToken.None);
        
        var childTicket = await getHandler.Handle(new GetTicketQuery(childTicketId), CancellationToken.None);
        childTicket.Parent.Should().NotBeNull();
        childTicket.Parent!.Id.Should().Be(1);
        childTicket.Parent.Title.Should().Be("Test Ticket 1");
        
        var updateParentCommand = new UpdateTicketParentCommand(
            Id: childTicketId,
            Parent: 2
        );

        ClearChangeTracker(); 
        await updateParentHandler.Handle(updateParentCommand, CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedChildTicket = await getHandler.Handle(new GetTicketQuery(childTicketId), CancellationToken.None);
        updatedChildTicket.Parent!.Id.Should().Be(2);
        
        var removeParentCommand = new UpdateTicketParentCommand(
            Id: childTicketId,
            Parent: null
        );

        await updateParentHandler.Handle(removeParentCommand, CancellationToken.None);
        
        var orphanTicket = await getHandler.Handle(new GetTicketQuery(childTicketId), CancellationToken.None);
        orphanTicket.Parent.Should().BeNull();
    }

    [Fact]
    public async Task FieldUpdateWorkflow_ShouldWorkEndToEnd()
    {
        var titleHandler = new UpdateTicketTitleCommandHandler(DbContext, GetLogger<UpdateTicketTitleCommandHandler>());
        var statusHandler = new UpdateTicketStatusCommandHandler(DbContext, GetLogger<UpdateTicketStatusCommandHandler>());
        var priorityHandler = new UpdateTicketPriorityCommandHandler(DbContext, GetLogger<UpdateTicketPriorityCommandHandler>());
        var getHandler = new GetTicketQueryHandler(DbContext);
        
        ClearChangeTracker(); 
        await titleHandler.Handle(new UpdateTicketTitleCommand(1, "New Title"), CancellationToken.None);
        
        ClearChangeTracker(); 
        await statusHandler.Handle(new UpdateTicketStatusCommand(1, TicketStatus.InProgress), CancellationToken.None);
        
        ClearChangeTracker(); 
        await priorityHandler.Handle(new UpdateTicketPriorityCommand(1, TicketPriority.High), CancellationToken.None);
        
        ClearChangeTracker(); 
        var updatedTicket = await getHandler.Handle(new GetTicketQuery(1), CancellationToken.None);
        updatedTicket.Title.Should().Be("New Title");
        updatedTicket.Status.Should().Be(TicketStatus.InProgress);
        updatedTicket.Priority.Should().Be(TicketPriority.High);
    }

    [Fact]
    public async Task LoginWorkflow_ShouldWorkEndToEnd()
    {
        var loginHandler = new LoginCommandHandler(DbContext, MockJwtService.Object, GetLogger<LoginCommandHandler>());
        
        var validLoginCommand = new LoginCommand("admin", "admin123");
        var loginResult = await loginHandler.Handle(validLoginCommand, CancellationToken.None);

        loginResult.User.Should().NotBeNull();
        loginResult.User.Username.Should().Be("admin");
        loginResult.Token.Should().Be("test-jwt-token");
        
        var invalidLoginCommand = new LoginCommand("admin", "wrongpassword");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => loginHandler.Handle(invalidLoginCommand, CancellationToken.None));
        
        var nonExistentUserCommand = new LoginCommand("nonexistent", "password");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => loginHandler.Handle(nonExistentUserCommand, CancellationToken.None));
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldHandleCorrectly()
    {
        var createHandler = new CreateTicketCommandHandler(DbContext, GetLogger<CreateTicketCommandHandler>());
        var updateHandler = new UpdateTicketCommandHandler(DbContext, GetLogger<UpdateTicketCommandHandler>());
        
        var createTasks = Enumerable.Range(1, 5).Select(async i =>
        {
            var command = new CreateTicketCommand(
                Priority: TicketPriority.Medium,
                Title: $"Concurrent Ticket {i}",
                Description: $"Description {i}",
                Author: "admin",
                Assignee: "user1",
                Parent: null
            );
            return await createHandler.Handle(command, CancellationToken.None);
        });

        var ticketIds = await Task.WhenAll(createTasks);
        
        ticketIds.Should().HaveCount(5);
        ticketIds.Should().OnlyContain(id => id > 0);
        ticketIds.Should().OnlyHaveUniqueItems();
        
        for (var index = 0; index < ticketIds.Length; index++)
        {
            ClearChangeTracker(); 
            var command = new UpdateTicketCommand(
                Id: ticketIds[index],
                Title: $"Updated Concurrent Ticket {index + 1}",
                Description: null,
                Author: null,
                Assignee: null,
                Parent: null
            );
            await updateHandler.Handle(command, CancellationToken.None);
        }
        
        foreach (var (id, index) in ticketIds.Select((id, index) => (id, index)))
        {
            var ticket = await DbContext.Tickets.FindAsync(id);
            ticket!.Title.Should().Be($"Updated Concurrent Ticket {index + 1}");
        }
    }
}
