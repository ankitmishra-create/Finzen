namespace FinanceManagement.Application.Exceptions
{
    public class TransactionLogNotFoundException : Exception
    {
        public TransactionLogNotFoundException(string message) : base(message) { }
    }
}
