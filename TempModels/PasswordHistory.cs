using System;
using System.Collections.Generic;

namespace SistemApp.TempModels;

public partial class PasswordHistory
{
    public int Id { get; set; }

    public string OldPassword { get; set; } = null!;

    public DateTime ChangedDate { get; set; }

    public int SystemHardwareId { get; set; }

    public virtual SystemHardware SystemHardware { get; set; } = null!;
}
