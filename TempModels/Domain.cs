using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class Domain
{
    public int Id { get; set; }

    public int? CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Company? Company { get; set; }

    public virtual ICollection<MailEntry> MailEntries { get; set; } = new List<MailEntry>();
}
