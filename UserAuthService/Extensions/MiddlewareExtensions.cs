using Serilog;

public static class MiddlewareExtensions
{
    public static void ConfigureMiddlewares(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix = string.Empty;
        });

        app.UseRouting();

        // Apply custom middlewares
        app.UseMiddleware<ApiKeyValidationMiddleware>();
        app.UseMiddleware<ApiLoggingMiddleware>();

        // Default middlewares
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseSerilogRequestLogging();

        // Map controllers
        app.MapControllers();
    }
}