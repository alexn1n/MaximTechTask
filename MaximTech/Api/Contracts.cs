namespace MaximTech.Api;

public sealed record DriverLocationRequest(int X, int Y);

public sealed record NearestDriverResponse(int Id, int X, int Y, double Distance);
