using CommonShared.Core.Domain.SharedKernel;

namespace CommonShared.Core.Domain;

public class Media : Entity<long>
{
    public String Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public Notification Notification { get; set; }
}