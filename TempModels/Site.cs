using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class Site
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();

    public virtual ICollection<MailEntry> MailEntries { get; set; } = new List<MailEntry>();

    public virtual ICollection<Personnel> Personnel { get; set; } = new List<Personnel>();

    public virtual ICollection<SystemHardware> SystemHardwares { get; set; } = new List<SystemHardware>();
}
