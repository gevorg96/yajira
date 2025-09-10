using Microsoft.EntityFrameworkCore;
using YetAnotherJira.Application.Commands;
using YetAnotherJira.Application.DAL;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Tests.Commands;

public class TicketRelationCommandTests : TestBase
{
    [Fact]
    public async Task AddTicketRelatesToCommand_ValidRelation_ShouldCreateRelation()
    {
        var handler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var command = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 2,
            RelationType: TicketRelationType.Blocks
        );
        
        await handler.Handle(command, CancellationToken.None);
         
        var relation = await DbContext.TicketRelations
            .FirstOrDefaultAsync(r => r.FromTaskId == 1 && r.ToTaskId == 2);
        
        relation.Should().NotBeNull();
        relation!.RelationType.Should().Be(TicketRelationType.Blocks);
    }

    [Fact]
    public async Task AddTicketRelatesToCommand_NonExistentFromTask_ShouldThrowTicketNotFoundException()
    {
        var handler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var command = new AddTicketRelatesToCommand(
            FromTaskId: 999,
            ToTaskId: 2,
            RelationType: TicketRelationType.Blocks
        );
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task AddTicketRelatesToCommand_NonExistentToTask_ShouldThrowTicketNotFoundException()
    {
        var handler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var command = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 999,
            RelationType: TicketRelationType.Blocks
        );
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task AddTicketRelatesToCommand_SelfRelation_ShouldThrowRelationToItselfException()
    {
        var handler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var command = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 1,
            RelationType: TicketRelationType.Blocks
        );
        
        var exception = await Assert.ThrowsAsync<RelationToItselfException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("1");
    }

    [Fact]
    public async Task AddTicketRelatesToCommand_DuplicateRelation_ShouldThrowRelationAlreadyExistsException()
    {
        DbContext.TicketRelations.Add(new TicketRelationDal
        {
            FromTaskId = 1,
            ToTaskId = 2,
            RelationType = TicketRelationType.Blocks,
        });
        await DbContext.SaveChangesAsync();

        var handler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var command = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 2,
            RelationType: TicketRelationType.Blocks 
        );
        
        var exception = await Assert.ThrowsAsync<RelationAlreadyExistsException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("1").And.Contain("2");
    }

    [Theory]
    [InlineData(TicketRelationType.Blocks)]
    [InlineData(TicketRelationType.Depends)]
    [InlineData(TicketRelationType.RelatedTo)]
    [InlineData(TicketRelationType.Duplicates)]
    public async Task AddTicketRelatesToCommand_DifferentRelationTypes_ShouldCreateCorrectRelation(TicketRelationType relationType)
    {
        var handler = new AddTicketRelatesToCommandHandler(DbContext, GetLogger<AddTicketRelatesToCommandHandler>());
        var command = new AddTicketRelatesToCommand(
            FromTaskId: 1,
            ToTaskId: 2,
            RelationType: relationType
        );
        
        await handler.Handle(command, CancellationToken.None);
        
        var relation = await DbContext.TicketRelations
            .FirstOrDefaultAsync(r => r.FromTaskId == 1 && r.ToTaskId == 2);
        
        relation.Should().NotBeNull();
        relation!.RelationType.Should().Be(relationType);
    }

    [Fact]
    public async Task DeleteTicketRelatesToCommand_ExistingRelations_ShouldDeleteRelations()
    {
        var relations = new[]
        {
            new TicketRelationDal { FromTaskId = 1, ToTaskId = 2, RelationType = TicketRelationType.Blocks },
            new TicketRelationDal { FromTaskId = 2, ToTaskId = 1, RelationType = TicketRelationType.Depends }
        };
        DbContext.TicketRelations.AddRange(relations);
        await DbContext.SaveChangesAsync();

        var handler = new DeleteTicketRelatesToCommandHandler(DbContext, GetLogger<DeleteTicketRelatesToCommandHandler>());
        var command = new DeleteTicketRelatesToCommand(Id: 1, RelatesTo: new[] { 2L });
        
        await handler.Handle(command, CancellationToken.None);
        
        var remainingRelations = await DbContext.TicketRelations
            .Where(r => (r.FromTaskId == 1 && r.ToTaskId == 2) || (r.FromTaskId == 2 && r.ToTaskId == 1))
            .ToListAsync();
        
        remainingRelations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTicketRelatesToCommand_NonExistentTicket_ShouldThrowTicketNotFoundException()
    {
        var handler = new DeleteTicketRelatesToCommandHandler(DbContext, GetLogger<DeleteTicketRelatesToCommandHandler>());
        var command = new DeleteTicketRelatesToCommand(Id: 999, RelatesTo: new[] { 1L });
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task DeleteTicketRelatesToCommand_NoExistingRelations_ShouldCompleteSuccessfully()
    {
        var handler = new DeleteTicketRelatesToCommandHandler(DbContext, GetLogger<DeleteTicketRelatesToCommandHandler>());
        var command = new DeleteTicketRelatesToCommand(Id: 1, RelatesTo: new[] { 2L });
        
        await handler.Handle(command, CancellationToken.None);
    }

    [Fact]
    public async Task DeleteTicketRelatesToCommand_MultipleRelations_ShouldDeleteAllSpecifiedRelations()
    {
        DbContext.Tickets.Add(new TicketDal
        {
            Id = 3,
            Title = "Test Ticket 3",
            Description = "Test Description 3",
            Author = "admin",
            Assignee = "user1",
            Priority = TicketPriority.Low,
            Status = TicketStatus.New,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        
        var relations = new[]
        {
            new TicketRelationDal { FromTaskId = 1, ToTaskId = 2, RelationType = TicketRelationType.Blocks },
            new TicketRelationDal { FromTaskId = 1, ToTaskId = 3, RelationType = TicketRelationType.RelatedTo }
        };
        DbContext.TicketRelations.AddRange(relations);
        await DbContext.SaveChangesAsync();

        var handler = new DeleteTicketRelatesToCommandHandler(DbContext, GetLogger<DeleteTicketRelatesToCommandHandler>());
        var command = new DeleteTicketRelatesToCommand(Id: 1, RelatesTo: new[] { 2L, 3L });
        
        await handler.Handle(command, CancellationToken.None);
        
        var remainingRelations = await DbContext.TicketRelations
            .Where(r => r.FromTaskId == 1)
            .ToListAsync();
        
        remainingRelations.Should().BeEmpty();
    }
}
