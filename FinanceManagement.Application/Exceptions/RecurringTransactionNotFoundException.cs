namespace FinanceManagement.Application.Exceptions
{
    public class RecurringTransactionNotFoundException : Exception
    {
        public RecurringTransactionNotFoundException(string message) : base(message)
        {

        }
    }
}
