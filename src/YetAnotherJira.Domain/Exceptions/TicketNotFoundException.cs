namespace YetAnotherJira.Domain.Exceptions;

public class TicketNotFoundException(long id) : Exception($"Ticket with id {id} not found");