namespace ECommerceBackend.Logging
{
    public class ApplicationLog
    {
        public int Id { get; set; }

        public string HttpMethod { get; set; } = string.Empty;

        public string RequestPath { get; set; } = string.Empty;

        public string QueryString { get; set; } = string.Empty;

        public DateTime RequestStartTime { get; set; }// DateTime is the data type(year, month, day, hour, minute, second, fractions of a second)

        public int ResponseStatusCode { get; set; }

        public long ExecutionDuration { get; set; }

        public string ClientIp { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;
    }
}