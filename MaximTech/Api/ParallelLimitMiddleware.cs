namespace MaximTech.Api;

public sealed class ParallelLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ParallelLimitMiddleware> _logger;
    private readonly int _limit;
    private int _current;

    public ParallelLimitMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ParallelLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _limit = configuration.GetValue<int>("Settings:ParallelLimit");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(_limit, "Settings:ParallelLimit");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (Interlocked.Increment(ref _current) > _limit)
        {
            Interlocked.Decrement(ref _current);
            _logger.LogWarning("Превышен лимит одновременных запросов {Limit}: {Method} {Path}",
                _limit, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            Interlocked.Decrement(ref _current);
        }
    }
}
