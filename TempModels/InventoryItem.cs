using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class InventoryItem
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int EquipmentTypeId { get; set; }

    public int IsArchived { get; set; }

    public int PersonnelId { get; set; }

    public string SerialNumber { get; set; } = null!;

    public int? SiteId1 { get; set; }

    public virtual EquipmentType EquipmentType { get; set; } = null!;

    public virtual ICollection<InventoryAssignmentHistory> InventoryAssignmentHistories { get; set; } = new List<InventoryAssignmentHistory>();

    public virtual Personnel Personnel { get; set; } = null!;

    public virtual Site? SiteId1Navigation { get; set; }
}
