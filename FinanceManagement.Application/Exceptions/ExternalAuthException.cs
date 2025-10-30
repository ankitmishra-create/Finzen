namespace FinanceManagement.Application.Exceptions
{
    public class ExternalAuthException : Exception
    {
        public ExternalAuthException(string message) : base(message) { }
    }
}
