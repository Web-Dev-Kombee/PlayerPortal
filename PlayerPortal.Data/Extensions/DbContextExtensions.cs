using Microsoft.EntityFrameworkCore;
using Shard.Commons;

namespace PlayerPortal.Data.Extensions
{
    public static class DbContextExtensions
    {
        public static Result SaveChangesWithResult<T>(this DbContext context, T data)
        {
            try
            {
                context.SaveChanges();
                return new Result(true, "Operation successful.");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Operation failed: {ex.Message}");
            }
        }
    }
}
