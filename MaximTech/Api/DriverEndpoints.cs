using MaximTech.Algorithms;
using MaximTech.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MaximTech.Api;

public static class DriverEndpoints
{
    private static readonly Lock LocatorLock = new();

    public static IServiceCollection AddDriverLocator(this IServiceCollection services, IConfiguration configuration)
    {
        var map = configuration.GetSection("DriverMap");
        var locator = DriverLocatorFactory.Create(
            map.GetValue("Algorithm", LocatorAlgorithm.GridSpiral),
            map.GetValue("Width", 1000),
            map.GetValue("Height", 1000));

        return services.AddSingleton(locator);
    }

    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPut("/drivers/{id:int}", UpdateLocation).WithName("UpdateDriverLocation");
        app.MapGet("/drivers/nearest", FindNearest).WithName("FindNearestDrivers");
        return app;
    }

    private static Results<NoContent, ValidationProblem, Conflict<ProblemDetails>> UpdateLocation(
        int id, DriverLocationRequest request, IDriverLocator locator)
    {
        if (!IsInside(locator, request.X, request.Y))
            return OutOfMap(locator);

        try
        {
            lock (LocatorLock)
                locator.AddOrUpdate(id, new Point(request.X, request.Y));
        }
        catch (InvalidOperationException e)
        {
            return TypedResults.Conflict(new ProblemDetails
            {
                Title = "Клетка уже занята",
                Detail = e.Message,
                Status = StatusCodes.Status409Conflict
            });
        }

        return TypedResults.NoContent();
    }

    private static Results<Ok<NearestDriverResponse[]>, ValidationProblem> FindNearest(
        int x, int y, IDriverLocator locator)
    {
        if (!IsInside(locator, x, y))
            return OutOfMap(locator);

        var order = new Point(x, y);
        IReadOnlyList<Driver> drivers;
        lock (LocatorLock)
            drivers = locator.FindNearest(order);

        var nearest = drivers
            .Select(d => new NearestDriverResponse(
                d.Id, d.Location.X, d.Location.Y, Math.Sqrt(Point.DistanceSquared(order, d.Location))))
            .ToArray();

        return TypedResults.Ok(nearest);
    }

    private static bool IsInside(IDriverLocator locator, int x, int y) =>
        (uint)x < (uint)locator.Width && (uint)y < (uint)locator.Height;

    private static ValidationProblem OutOfMap(IDriverLocator locator) =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
        {
            ["x"] = [$"Допустимо 0 <= X < {locator.Width}."],
            ["y"] = [$"Допустимо 0 <= Y < {locator.Height}."]
        });
}
