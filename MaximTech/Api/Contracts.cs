using MaximTech.Domain;

namespace MaximTech.Api;

public sealed record DriverRequest(int Id, int X, int Y);

public sealed record OrderRequest(int Id, int X, int Y);

public sealed record MessageResponse(string Message);

public sealed record OrderResponse(int DriverId, Point DriverLocation, int RouteLength, IReadOnlyList<Point> Route);
