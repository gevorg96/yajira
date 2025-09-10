namespace YetAnotherJira.Domain.Exceptions;

public class RelationToItselfException(long id): Exception($"Relation to itself is not allowed for ticket with id {id}");