using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class MailPasswordHistory
{
    public int Id { get; set; }

    public string OldPassword { get; set; } = null!;

    public DateTime ChangedDate { get; set; }

    public int MailEntryId { get; set; }

    public virtual MailEntry MailEntry { get; set; } = null!;
}
