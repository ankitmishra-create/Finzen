# Recurring Transaction Processor

This processor automatically executes recurring transactions based on their scheduled frequency. It runs as a background service and processes transactions that are due for execution.

## Features

- **Automatic Processing**: Runs continuously in the background
- **Configurable Intervals**: Set execution frequency via configuration
- **Error Handling**: Comprehensive logging and error management
- **Manual Trigger**: Option to manually trigger processing for testing
- **Database Integration**: Uses Entity Framework Core for data access

## Configuration

The processor can be configured via `appsettings.json`:

```json
{
  "RecurringTransactionProcessor": {
    "ExecutionIntervalHours": 1,
    "MaxRetryAttempts": 3,
    "RetryDelayMinutes": 5
  }
}
```

### Configuration Options

- `ExecutionIntervalHours`: How often the processor runs (default: 1 hour)
- `MaxRetryAttempts`: Maximum retry attempts for failed transactions (default: 3)
- `RetryDelayMinutes`: Delay between retry attempts (default: 5 minutes)

## How It Works

1. **Scheduled Execution**: The background service runs at configured intervals
2. **Transaction Discovery**: Finds recurring transactions due for execution
3. **Transaction Creation**: Creates new transactions based on recurring templates
4. **Schedule Update**: Updates next execution dates based on frequency
5. **Status Management**: Handles end dates and inactive transactions

## Recurrence Frequencies

The processor supports the following frequencies:

- **Daily**: Executes every day
- **Weekly**: Executes every 7 days
- **Monthly**: Executes every month
- **Quarterly**: Executes every 3 months
- **Yearly**: Executes every year

## Running the Processor

### As a Background Service

```bash
dotnet run --project FinanceManagement.Processor
```

### Manual Processing (for testing)

```bash
dotnet run --project FinanceManagement.Processor -- ManualProcessor
```

## Database Requirements

The processor requires the following database tables:

- `RecurringTransactions`: Stores recurring transaction templates
- `Transactions`: Stores individual transaction records
- `Users`: User information
- `Categories`: Transaction categories

## Logging

The processor provides comprehensive logging:

- **Information**: Processing start/end, transaction counts
- **Warning**: Non-critical issues
- **Error**: Processing failures with full exception details
- **Debug**: Detailed execution information

## Error Handling

- **Retry Logic**: Failed transactions are retried based on configuration
- **Graceful Degradation**: Individual transaction failures don't stop the entire process
- **Comprehensive Logging**: All errors are logged with context
- **Transaction Rollback**: Database changes are rolled back on critical failures

## Monitoring

Monitor the processor through:

1. **Application Logs**: Check console output or log files
2. **Database Queries**: Monitor transaction creation and recurring transaction updates
3. **Performance Metrics**: Track processing times and transaction volumes

## Troubleshooting

### Common Issues

1. **No Transactions Processed**
   - Check if recurring transactions exist and are active
   - Verify `NextTransactionDate` is set correctly
   - Ensure `IsActive` flag is true

2. **Database Connection Issues**
   - Verify connection string in `appsettings.json`
   - Check database server availability
   - Ensure proper permissions

3. **Processing Errors**
   - Check application logs for detailed error messages
   - Verify data integrity in recurring transactions
   - Check for missing foreign key references

### Debug Mode

Enable debug logging by updating `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "FinanceManagement.Processor": "Debug"
    }
  }
}
```

## Development

### Adding New Features

1. **New Recurrence Frequencies**: Update `RecurrenceFrequency` enum and calculation logic
2. **Custom Processing Logic**: Extend `RecurringTransactionProcessor` class
3. **Additional Configuration**: Add new settings to `RecurringTransactionProcessorSettings`

### Testing

Use the manual processor for testing:

```bash
dotnet run --project FinanceManagement.Processor -- ManualProcessor
```

This allows you to trigger processing on-demand and verify results.
