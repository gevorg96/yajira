using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YetAnotherJira.Application.Commands;
using YetAnotherJira.Application.Enums;
using YetAnotherJira.Application.Queries;
using YetAnotherJira.Controllers.Mappers;
using YetAnotherJira.Domain.Enums;
using YetAnotherJira.Models.V1.Requests;
using YetAnotherJira.Models.V1.Responses;

namespace YetAnotherJira.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/ticket")]
public class TicketController(IMediator mediator): ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<V1GetTicketResponse>> GetTicket(long id, CancellationToken token)
    {
        var res = await mediator.Send(new GetTicketQuery(id), token);

        return Ok(new V1GetTicketResponse
        {
            Data = TicketMapper.MapSingle(res)
        });
    }

    [HttpPost("filter")]
    public async Task<ActionResult<V1GetTicketsResponse>> GetTickets([FromBody] V1GetTicketsRequest request,
        CancellationToken token)
    {
        var res = await mediator.Send(new GetTicketsQuery(
            Enum.TryParse<SortBy>(request.SortBy, out var sortBy) ? sortBy : SortBy.CreatedAt,
            Enum.TryParse<SortOrder>(request.SortOrder, out var sortOrder) ? sortOrder : SortOrder.Asc,
            request.Page,
            request.PageSize,
            request.Search,
            request.Statuses?.Select(Enum.Parse<TicketStatus>).ToArray(),
            request.Priorities?.Select(Enum.Parse<TicketPriority>).ToArray(),
            request.Authors,
            request.Assignees
            ), token);

        return Ok(new V1GetTicketsResponse
        {
            Tickets = res.Items.Select(TicketMapper.MapQuery).ToArray(),
            Total = res.Total,
            Page = request.Page,
            PageSize = res.Items.Length
        });
    }
    
    [HttpPost("create")]
    public async Task<IActionResult> CreateTicket([FromBody] V1CreateTicketRequest request,
        CancellationToken token)
    {
        var id = await mediator.Send(new CreateTicketCommand(
            Enum.Parse<TicketPriority>(request.Priority), 
            request.Title,
            request.Description,
            request.Author,
            request.Assignee,
            request.Parent), token);

        return Created($"api/v1/ticket/{id}", new { id = id});
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(long id, [FromBody] V1UpdateTicketRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketCommand(id, request.Title, request.Description, request.Author,
            request.Assignee, request.Parent), token);

        return NoContent();
    }
    
    [HttpPut("status/{id}")]
    public async Task<IActionResult> UpdateTicketStatus(long id, [FromBody] V1UpdateTicketStatusRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketStatusCommand(id, Enum.Parse<TicketStatus>(request.Status)), token);

        return NoContent();
    }
    
    [HttpPut("priority/{id}")]
    public async Task<IActionResult> UpdateTicketPriority(long id, [FromBody] V1UpdateTicketPriorityRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketPriorityCommand(id, Enum.Parse<TicketPriority>(request.Priority)), token);

        return NoContent();
    }
    
    [HttpPut("executor/{id}")]
    public async Task<IActionResult> UpdateTicketAssignee(long id, [FromBody] V1UpdateTicketAssigneeRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketAssigneeCommand(id, request.Assignee), token);

        return NoContent();
    }
    
    [HttpPut("author/{id}")]
    public async Task<IActionResult> UpdateTicketAuthor(long id, [FromBody] V1UpdateTicketAuthorRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketAuthorCommand(id, request.Author), token);

        return NoContent();
    }
    
    [HttpPut("relates-to/add/{id}")]
    public async Task<IActionResult> AddTicketRelatesTo(long id, [FromBody] V1AddTicketRelatesToRequest request,
        CancellationToken token)
    {
        foreach (var relatedId in request.RelatesTo)
        {
            await mediator.Send(new AddTicketRelatesToCommand(id, relatedId, Enum.Parse<TicketRelationType>(request.RelationType)), token);
        }

        return NoContent();
    }
    
    [HttpPut("relates-to/delete/{id}")]
    public async Task<IActionResult> DeleteTicketRelatesTo(long id, [FromBody] V1DeleteTicketRelatesToRequest request,
        CancellationToken token)
    {
        await mediator.Send(new DeleteTicketRelatesToCommand(id, request.RelatesTo), token);

        return NoContent();
    }

    [HttpPut("parent/{id}")]
    public async Task<IActionResult> UpdateTicketParent(long id, [FromBody] V1UpdateTicketParentRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketParentCommand(id, request.Parent), token);

        return NoContent();
    }
    
    [HttpPut("header/{id}")]
    public async Task<IActionResult> UpdateTicketHeader(long id, [FromBody] V1UpdateTicketTitleRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketTitleCommand(id, request.Title), token);

        return NoContent();
    }
    
    [HttpPut("description/{id}")]
    public async Task<IActionResult> UpdateTicketDescription(long id, [FromBody] V1UpdateTicketDescriptionRequest request,
        CancellationToken token)
    {
        await mediator.Send(new UpdateTicketDescriptionCommand(id, request.Description), token);

        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(long id, CancellationToken token)
    {
        await mediator.Send(new DeleteTicketCommand(id), token);

        return NoContent();
    }
}