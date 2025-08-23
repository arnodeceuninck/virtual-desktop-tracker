using System.Collections.Generic;
using VirtualDesktopHelper.Models;

namespace VirtualDesktopHelper.Interfaces
{
    /// <summary>
    /// Interface for consolidating desktop usage entries based on various criteria.
    /// </summary>
    public interface IUsageConsolidationService
    {
        /// <summary>
        /// Applies all enabled consolidation strategies to the usage entries.
        /// </summary>
        /// <param name="entries">The original usage entries.</param>
        /// <returns>Consolidated usage entries.</returns>
        List<DesktopUsageEntry> ConsolidateUsageEntries(List<DesktopUsageEntry> entries);
    }
}
