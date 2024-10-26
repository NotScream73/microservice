namespace MicroService
{
    public class TraceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TraceLoggingMiddleware> _logger;
        private readonly string _project;

        public TraceLoggingMiddleware(RequestDelegate next, ILogger<TraceLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _project = "MicroService";
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = context.Request.Headers["X-Trace-Id"].ToString();
            var method = context.Request.Method;
            var path = context.Request.Path;

            _logger.LogInformation("{ProjectName}. TraceId: {TraceId}. {Method} {Path}", _project, traceId, method, path);

            await _next(context);
        }

    }
}
