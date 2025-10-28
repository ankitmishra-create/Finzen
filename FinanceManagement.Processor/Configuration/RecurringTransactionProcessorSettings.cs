namespace FinanceManagement.Processor.Configuration
{
    public class RecurringTransactionProcessorSettings
    {
        public const string SectionName = "RecurringTransactionProcessor";

        public int ExecutionIntervalHours { get; set; } = 1;
        public int MaxRetryAttempts { get; set; } = 3;
        public int RetryDelayMinutes { get; set; } = 5;
    }
}
