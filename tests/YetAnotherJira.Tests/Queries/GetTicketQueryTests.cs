using YetAnotherJira.Application.DAL;
using YetAnotherJira.Application.Queries;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Domain.Exceptions;

namespace YetAnotherJira.Tests.Queries;

public class GetTicketQueryTests : TestBase
{
    [Fact]
    public async Task Handle_ExistingTicket_ShouldReturnTicketWithDetails()
    {
        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 1);
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Ticket 1");
        result.Description.Should().Be("Test Description 1");
        result.Author.Should().Be("admin");
        result.Assignee.Should().Be("user1");
        result.Priority.Should().Be(TicketPriority.High);
        result.Status.Should().Be(TicketStatus.New);
        result.IsDeleted.Should().BeFalse();
        result.Parent.Should().BeNull();
        result.RelatedTickets.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_TicketWithParent_ShouldReturnParentInformation()
    {
        var ticket = await DbContext.Tickets.FindAsync(1L);
        ticket!.ParentTaskId = 2;
        await DbContext.SaveChangesAsync();

        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 1);
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.Parent.Should().NotBeNull();
        result.Parent!.Id.Should().Be(2);
        result.Parent.Title.Should().Be("Test Ticket 2");
    }

    [Fact]
    public async Task Handle_TicketWithRelations_ShouldReturnRelatedTickets()
    {
        DbContext.TicketRelations.Add(new TicketRelationDal
        {
            FromTaskId = 1,
            ToTaskId = 2,
            RelationType = TicketRelationType.Blocks,
        });
        await DbContext.SaveChangesAsync();

        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 1);
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.RelatedTickets.Should().NotBeEmpty();
        result.RelatedTickets.Should().HaveCount(1);
        
        var relation = result.RelatedTickets.First();
        relation.RelatedTicket.Id.Should().Be(2);
        relation.RelatedTicket.Title.Should().Be("Test Ticket 2");
        relation.RelationType.Should().Be(TicketRelationType.Blocks);
    }

    [Fact]
    public async Task Handle_NonExistentTicket_ShouldThrowTicketNotFoundException()
    {
        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 999);
        
        var exception = await Assert.ThrowsAsync<TicketNotFoundException>(
            () => handler.Handle(query, CancellationToken.None));
        
        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task Handle_DeletedTicket_ShouldReturnDeletedTicket()
    {
        var ticket = await DbContext.Tickets.FindAsync(1L);
        ticket!.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 1);
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_TicketWithMultipleRelations_ShouldReturnAllRelations()
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
            new TicketRelationDal { FromTaskId = 1, ToTaskId = 3, RelationType = TicketRelationType.RelatedTo },
            new TicketRelationDal { FromTaskId = 2, ToTaskId = 1, RelationType = TicketRelationType.Depends }
        };
        DbContext.TicketRelations.AddRange(relations);
        await DbContext.SaveChangesAsync();

        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 1);
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.RelatedTickets.Should().HaveCount(2); 
        
        var relationTypes = result.RelatedTickets.Select(r => r.RelationType).ToList();
        relationTypes.Should().Contain(TicketRelationType.Blocks);
        relationTypes.Should().Contain(TicketRelationType.RelatedTo);
        
    }

    [Theory]
    [InlineData(TicketRelationType.Blocks)]
    [InlineData(TicketRelationType.Depends)]
    [InlineData(TicketRelationType.RelatedTo)]
    [InlineData(TicketRelationType.Duplicates)]
    public async Task Handle_DifferentRelationTypes_ShouldReturnCorrectRelationType(TicketRelationType relationType)
    {
        DbContext.TicketRelations.Add(new TicketRelationDal
        {
            FromTaskId = 1,
            ToTaskId = 2,
            RelationType = relationType,
        });
        await DbContext.SaveChangesAsync();

        var handler = new GetTicketQueryHandler(DbContext);
        var query = new GetTicketQuery(Id: 1);
        
        var result = await handler.Handle(query, CancellationToken.None);
        
        result.RelatedTickets.Should().HaveCount(1);
        result.RelatedTickets.First().RelationType.Should().Be(relationType);
    }
}
