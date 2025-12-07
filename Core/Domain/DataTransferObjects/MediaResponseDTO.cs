using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace microservices_project.Core.Domain.DataTransferObjects;

public class MediaResponseDTO
{
    public MediaResponseDTO(Media media)
    {
        Source = media.Source;
        CreatedAt = media.CreatedAt;
    }

    public String Source { get; set; }
    public DateTime CreatedAt { get; set; }
}