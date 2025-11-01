using Microsoft.AspNetCore.Builder;

namespace DynamicHealthCheck.Abstractions;

public interface IMiddlewarePipelineContributor
{
    void Configure(IApplicationBuilder app);
}
