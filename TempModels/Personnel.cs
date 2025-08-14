using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class Personnel
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int SiteId { get; set; }

    public int IsActive { get; set; }

    public virtual ICollection<InventoryAssignmentHistory> InventoryAssignmentHistories { get; set; } = new List<InventoryAssignmentHistory>();

    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public virtual ICollection<MailEntry> MailEntries { get; set; } = new List<MailEntry>();

    public virtual Site Site { get; set; } = null!;
}
