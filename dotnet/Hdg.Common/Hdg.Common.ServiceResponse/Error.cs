using Microsoft.Extensions.Logging;

namespace Hdg.Common.ServiceResponse
{
    public class Error(string message, LogLevel logLevel = LogLevel.Error,
        Exception? exception = null, HttpResponseMessage? httpResponse = null)
    {
        private string _message = message ?? exception?.Message
            ?? httpResponse?.StatusCode.ToString() ?? "Unspecified error";
        private LogLevel _logLevel = logLevel;
        private Exception? _exception = exception;
        private HttpResponseMessage? _httpResponse = httpResponse;

        public string Message => _message;
        public LogLevel LogLevel => _logLevel;
        public Exception? Exception => _exception;
        public HttpResponseMessage? HttpResponse => _httpResponse;

        public void SetMessage(string message) => _message = message;
        public void SetLogLevel(LogLevel logLevel) => _logLevel = logLevel;
        public void SetException(Exception? exception) => _exception = exception;
        public void SetHttpResponse(HttpResponseMessage? httpResponse) => _httpResponse = httpResponse;

        /// <summary>
        /// Omits 'HttpResponseMessage.Content'
        /// </summary>
        public override string ToString()
        {
            string aggregateError = $"{_logLevel} error: {_message}";

            if (_exception != null)
                aggregateError += "\nException: " + _exception.ToString();

            if (_httpResponse != null)
                aggregateError += "\nStatusCode: " + _httpResponse.StatusCode.ToString();

            return aggregateError;
        }

        /// <summary>
        /// Includes 'HttpResponseMessage.Content'
        /// </summary>
        public async Task<string> ToStringAsync()
        {
            string aggregateError = ToString();
            if (_httpResponse != null)
            {
                string content = await _httpResponse.Content.ReadAsStringAsync();
                aggregateError += $"\nHttpResponse: {content}";
            }
            return aggregateError;
        }
    }
}
