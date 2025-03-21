namespace MindCare.Services
{
    public class ActivityTrackingMiddleware
    {
        private readonly RequestDelegate _next;

        public ActivityTrackingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserActivityService activityService)
        {
            // Continue with the request first
            await _next(context);

            // Check if the user is authenticated
            if (context.User.Identity.IsAuthenticated &&
                !context.Request.Path.StartsWithSegments("/api") &&
                context.Response.StatusCode < 400)
            {
                var username = context.User.Identity.Name;
                var path = context.Request.Path.Value;

                // Log activity after request is done
                await activityService.LogActivity(
                    username,
                    "PageView",
                    $"Viewed {path}");
            }
        }
    }

    // Extension method to use in Program.cs or Startup.cs
    public static class ActivityTrackingMiddlewareExtensions
    {
        public static IApplicationBuilder UseActivityTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActivityTrackingMiddleware>();
        }
    }
}
