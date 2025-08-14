using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class Company
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Domain> Domains { get; set; } = new List<Domain>();

    public virtual ICollection<Site> Sites { get; set; } = new List<Site>();

    public virtual ICollection<SystemHardware> SystemHardwares { get; set; } = new List<SystemHardware>();
}
