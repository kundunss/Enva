using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class SystemHardware
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string Description { get; set; } = null!;

    public int EquipmentTypeId { get; set; }

    public string IpAddress { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int SiteId { get; set; }

    public string Username { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual EquipmentType EquipmentType { get; set; } = null!;

    public virtual ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    public virtual Site Site { get; set; } = null!;
}
