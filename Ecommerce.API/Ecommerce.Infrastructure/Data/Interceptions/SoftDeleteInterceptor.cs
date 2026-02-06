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
            // check if the event data is null
            if (eventData.Context is null)
                return result;

            // make the save changes
            foreach (var entry in eventData.Context.ChangeTracker.Entries())
            {
                if (entry is null
                    || entry.State != EntityState.Deleted
                    || !(entry.Entity is ISoftDelete entity))
                    continue;
                entry.State = EntityState.Modified;
                entity.Delete();
            }
            return result;
        }
    }
}