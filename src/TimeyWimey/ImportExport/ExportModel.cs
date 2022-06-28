using TimeyWimey.Model;

namespace TimeyWimey.ImportExport
{
#nullable disable
    public class ExportModel
    {
        public DayExport[] Days { get; init; }
        public TimeActivityExport[] Activities { get; init; }
        public TimeEntryExport[] Entries { get; init; }
        public TimeCodeExport[] TimeCodes { get; init; }
        public TimeCodeSystemExport[] CodeSystems { get; init; }
        public TimeActivityTimeCodeExport[] TimeActivityTimeCode { get; set; }
    }
}
