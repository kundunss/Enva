using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class InventoryAssignmentHistory
{
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    public int PersonnelId { get; set; }

    public DateTime AssignedDate { get; set; }

    public virtual InventoryItem InventoryItem { get; set; } = null!;

    public virtual Personnel Personnel { get; set; } = null!;
}
