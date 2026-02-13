using Ecommerce.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ecommerce.Infrastructure.Data.Interceptions
{
    public class SoftDeleteInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges( 
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            ProcessSoftDelete(eventData.Context);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ProcessSoftDelete(eventData.Context);
            return ValueTask.FromResult(result);
        }

        private void ProcessSoftDelete(DbContext? context)
        {
            // check if the context is null
            if (context is null)
                return;

            // make the save changes
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry is null
                    || entry.State != EntityState.Deleted
                    || !(entry.Entity is ISoftDelete entity))
                    continue;
                entry.State = EntityState.Modified;
                entity.Delete();
            }
        }
    }
}