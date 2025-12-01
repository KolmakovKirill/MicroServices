using microservices_project.Core.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;

namespace microservices_project.Core.Domain.Channels;

public class Email : Entity<long>, IChannel // Скорее всего переделаю, надо придумать формат M2M связи, для этого скорее всего надо будет убрать валидаторы с db, а поставить их на CRUD, чтобы валидировались по создаваемому типу, а по-другому убрать возможности добавления.
{
    // [Required(ErrorMessage = "Email обязателен!")]
    // [EmailAddress(ErrorMessage = "Некорректный формат Email!")]
    // [MaxLength(254)]
    public string Source { get; set; } = null!;
}