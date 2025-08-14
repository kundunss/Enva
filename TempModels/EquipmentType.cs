using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class EquipmentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public virtual ICollection<SystemHardware> SystemHardwares { get; set; } = new List<SystemHardware>();
}
