namespace microservices_project.Core.Domain.SharedKernel;

public abstract class Entity<TKey> : Entity
{
    public TKey Id { get; set; }
}