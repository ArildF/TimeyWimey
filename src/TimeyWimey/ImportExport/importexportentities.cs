namespace TimeyWimey.ImportExport;

public record DayExport(int Id, DateOnly Date);
public record TimeActivityExport(int Id, string? Name, DateTime Created, string? Color, DateTime? LastUsed);

public record TimeActivityTimeCodeExport(int ActivitiesId, int TimeCodesId);

public record TimeCodeExport(int Id, string Code, string? Description, int SystemId);

public record TimeCodeSystemExport(int Id, string Name, string Description);

public record TimeEntryExport(int Id, TimeOnly Start, TimeOnly End, string? Title,
    string? Color, int DayId, string? Notes, int? ActivityId);
