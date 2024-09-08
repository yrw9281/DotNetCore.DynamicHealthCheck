using Microsoft.AspNetCore.Builder;

namespace DynamicHealthCheck;

public static class MiddlewareManager
{
    private static readonly List<Action<IApplicationBuilder>> Middlewares = [];

    public static void RegisterMiddleware(Action<IApplicationBuilder> middleware)
    {
        Middlewares.Add(middleware);
    }

    public static void ApplyMiddlewares(IApplicationBuilder builder)
    {
        foreach (var middleware in Middlewares)
        {
            middleware(builder);
        }
    }
}
