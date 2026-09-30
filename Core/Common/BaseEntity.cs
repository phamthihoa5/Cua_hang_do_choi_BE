using System.ComponentModel.DataAnnotations;

namespace Core.Common;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}