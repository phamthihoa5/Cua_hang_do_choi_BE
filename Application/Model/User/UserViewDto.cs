using Core.Common;
using Core.Entities;
using static Core.Entities.Enum;

namespace Application.Model.User;

public class UserViewDto
{
    public Guid Id { get; set; }


    /// <summary>
    /// Họ và tên đầy đủ
    /// </summary>
    public string FullName { get; set; } = string.Empty;

}
