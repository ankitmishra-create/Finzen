namespace FinanceManagement.Application.Exceptions
{
    public class EmailSendException : Exception
    {
        public EmailSendException(string message) : base(message) { }
        public EmailSendException(string message, Exception innerException) : base(message, innerException) { }
    }
    public class TokenGenerationException : Exception
    {
        public TokenGenerationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
