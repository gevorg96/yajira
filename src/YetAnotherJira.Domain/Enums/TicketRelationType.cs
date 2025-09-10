namespace YetAnotherJira.Domain.Enums;

public enum TicketRelationType
{
    RelatedTo,      // общая связь
    Blocks,         // блокирует выполнение
    BlockedBy,      // заблокирована
    Depends,        // зависит от
    Duplicates,     // дублирует
    DuplicatedBy,   // дублируется
    Follows,        // следует после
    Precedes        // предшествует
}