using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class MailEntry
{
    public int Id { get; set; }

    public int PersonnelId { get; set; }

    public string EmailUsername { get; set; } = null!;

    public int DomainId { get; set; }

    public string Password { get; set; } = null!;

    public string EmailAddress { get; set; } = null!;

    public int IsArchived { get; set; }

    public int? SiteId1 { get; set; }

    public virtual Domain Domain { get; set; } = null!;

    public virtual ICollection<MailPasswordHistory> MailPasswordHistories { get; set; } = new List<MailPasswordHistory>();

    public virtual Personnel Personnel { get; set; } = null!;

    public virtual Site? SiteId1Navigation { get; set; }
}
