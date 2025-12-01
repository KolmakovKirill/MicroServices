using microservices_project.Core.Domain.SharedKernel;

namespace microservices_project.Core.Domain;

public class User : Entity<long>
{
    public String Username { get; set; }
    
}