namespace FinanceManagement.Application.Exceptions
{
    public class InvalidCurrencyException : Exception
    {
        public InvalidCurrencyException(string message) : base(message) { }
    }

    public class CurrencyConversionException : Exception
    {
        public CurrencyConversionException(string message) :base(message) { }
        public CurrencyConversionException(string message, Exception ex) : base(message) { }

        public CurrencyConversionException(string message, HttpRequestException ex) : base(message) { }
    }

    public class DataRetrievalException : Exception
    {
        public DataRetrievalException(string message, Exception ex) : base(message, ex) { }
    }
}
