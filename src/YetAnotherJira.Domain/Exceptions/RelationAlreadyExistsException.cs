namespace YetAnotherJira.Domain.Exceptions;

public class RelationAlreadyExistsException(long fromTaskId, long toTaskId): Exception($"Relation between {fromTaskId} and {toTaskId} already exists");