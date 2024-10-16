using Microsoft.Extensions.Logging;

namespace Hdg.Common.ServiceResponse
{
    public class ServiceResponse<TPayload>
    {
        private TPayload? _payload;
        private List<Error>? _errors; // Lazy list, null when count == 0

        public ServiceResponse(TPayload? payload = default, params Error[] errors)
        {
            _payload = payload;
            if (errors.Length != 0) _errors = new List<Error>(errors);
        }

        public TPayload? Payload => _payload;
        public IReadOnlyList<Error> Errors => _errors?.AsReadOnly() ?? null!;

        public bool HasPayload => _payload != null;
        public bool HasErrors => _errors != null;

        public void SetPayload(TPayload? payload) => _payload = payload;
        public void AddError(Error error) => (_errors ??= []).Add(error);
        public void AddErrors(IEnumerable<Error> errors) => (_errors ??= []).AddRange(errors);
        public void SetErrors(IEnumerable<Error> errors) => _errors = errors.Any() ? new(errors) : null;
        public bool RemoveError(Error error) => _errors?.Remove(error) ?? false;
        public int RemoveErrors(Predicate<Error> match) => _errors?.RemoveAll(match) ?? 0;
        public void ClearErrors() => _errors = null;

        public string GetJoinedErrorString(string delimiter = ", ", LogLevel? minimumLogLevel = null)
        {
            if (_errors == null) return "No errors.";

            IEnumerable<Error> filteredErrors = minimumLogLevel.HasValue
                ? _errors.Where(e => e.LogLevel >= minimumLogLevel.Value)
                : _errors;

            return string.Join(delimiter, filteredErrors.Select(e => e.ToString()));
        }

        public void LogMessages<TService>(ILogger<TService> logger, LogLevel? minimumLogLevel = null)
        {
            ArgumentNullException.ThrowIfNull(logger);
            if (_errors == null) return;

            var filteredErrors = minimumLogLevel.HasValue
                ? _errors.Where(e => e.LogLevel >= minimumLogLevel.Value)
                : _errors;

            if (!filteredErrors.Any()) return;

            LogLevel highestLogLevel = filteredErrors.Max(e => e.LogLevel);
            Exception? exception = filteredErrors.FirstOrDefault(e => e.Exception != null)?.Exception;
            string joinedErrors = "Errors: " + GetJoinedErrorString(minimumLogLevel: minimumLogLevel);

            logger.Log(highestLogLevel, exception, joinedErrors);
        }

        public void LogMessagesIndividually<TService>(ILogger<TService> logger, LogLevel? minimumLogLevel = null)
        {
            ArgumentNullException.ThrowIfNull(logger);
            if (_errors == null) return;

            foreach (var error in _errors.Where(e => !minimumLogLevel.HasValue || e.LogLevel >= minimumLogLevel.Value))
            {
                logger.Log(error.LogLevel, error.Exception, error.ToString());
            }
        }
    }
}
