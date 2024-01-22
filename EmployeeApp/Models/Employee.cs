namespace EmployeeApp.Models
{
        public class Employee 
        {

            public Guid Id { get; set; }
            public string? EmployeeName { get; set; }
            public DateTime StarTimeUtc { get; set; }
            public DateTime EndTimeUtc { get; set; }
            public string? EntryNotes { get; set; }
            public DateTime? DeletedOn { get; set; }
            public TimeSpan? TotalTimeWorked => EndTimeUtc.Subtract(StarTimeUtc);

    }
}
