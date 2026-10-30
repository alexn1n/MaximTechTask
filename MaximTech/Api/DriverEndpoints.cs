using MaximTech.Algorithms;
using MaximTech.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MaximTech.Api;

public static class DriverEndpoints
{
    private const string InvalidCoordinates = "Координаты некорректны";
    private const string CellOccupied = "Здесь уже находится другой водитель";
    private const string NoDrivers = "Свободных водителей нет";

    private static readonly Lock LocatorLock = new();

    public static IServiceCollection AddDriverServices(this IServiceCollection services, IConfiguration configuration)
    {
        var map = configuration.GetSection("MapSettings");
        services.AddSingleton<IDriverLocator>(new GridSpiralLocator(map.GetValue<int>("N"), map.GetValue<int>("M")));

        services.AddHttpClient<RandomNumberClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.randomnumberapi.com/");
            client.Timeout = TimeSpan.FromSeconds(3);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapDriverEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/drivers", UpdateDriver).WithName("UpdateDriver");
        app.MapPost("/orders", AssignDriver).WithName("AssignDriver");
        return app;
    }

    private static Results<Ok<MessageResponse>, BadRequest<MessageResponse>, Conflict<MessageResponse>> UpdateDriver(
        DriverRequest request, IDriverLocator locator, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(DriverEndpoints));
        var location = new Point(request.X, request.Y);

        if (!locator.IsInside(location))
        {
            bool removed;
            lock (LocatorLock)
                removed = locator.Remove(request.Id);

            logger.LogWarning("Водитель {DriverId}: некорректные координаты {Location}, прежние координаты удалены: {Removed}",
                request.Id, location, removed);
            return TypedResults.BadRequest(new MessageResponse(InvalidCoordinates));
        }

        bool added;
        try
        {
            lock (LocatorLock)
                added = locator.AddOrUpdate(request.Id, location);
        }
        catch (InvalidOperationException)
        {
            logger.LogWarning("Водитель {DriverId}: клетка {Location} занята", request.Id, location);
            return TypedResults.Conflict(new MessageResponse(CellOccupied));
        }

        logger.LogInformation("Водитель {DriverId}: координаты {Location} {Action}",
            request.Id, location, added ? "добавлены" : "изменены");
        return TypedResults.Ok(new MessageResponse(added ? "Координаты успешно добавлены" : "Координаты успешно изменены"));
    }

    private static async Task<Results<Ok<OrderResponse>, BadRequest<MessageResponse>, NotFound<MessageResponse>>> AssignDriver(
        OrderRequest request, IDriverLocator locator, RandomNumberClient randomNumbers, ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(typeof(DriverEndpoints));
        var order = new Point(request.X, request.Y);

        if (!locator.IsInside(order))
        {
            logger.LogWarning("Заказ {OrderId}: некорректные координаты {Location}", request.Id, order);
            return TypedResults.BadRequest(new MessageResponse(InvalidCoordinates));
        }

        IReadOnlyList<Driver> nearest;
        lock (LocatorLock)
            nearest = locator.FindNearest(order);

        if (nearest.Count == 0)
        {
            logger.LogWarning("Заказ {OrderId}: свободных водителей нет", request.Id);
            return TypedResults.NotFound(new MessageResponse(NoDrivers));
        }

        var number = await randomNumbers.NextAsync(1, nearest.Count, cancellationToken);
        var driver = nearest[number - 1];
        var route = DriverRoute.Build(driver.Location, order);

        logger.LogInformation("Заказ {OrderId}: назначен водитель {DriverId} ({Number} из {Count}), маршрут {Length}",
            request.Id, driver.Id, number, nearest.Count, route.Count - 1);
        return TypedResults.Ok(new OrderResponse(driver.Id, driver.Location, route.Count - 1, route));
    }
}
