namespace MaximTech.Domain;

public static class DriverRoute
{
    public static IReadOnlyList<Point> Build(Point from, Point to)
    {
        var route = new List<Point>(Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y) + 1) { from };
        var x = from.X;
        var y = from.Y;

        while (x != to.X)
        {
            x += Math.Sign(to.X - x);
            route.Add(new Point(x, y));
        }

        while (y != to.Y)
        {
            y += Math.Sign(to.Y - y);
            route.Add(new Point(x, y));
        }

        return route;
    }
}
