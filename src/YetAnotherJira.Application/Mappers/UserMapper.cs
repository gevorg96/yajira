using YetAnotherJira.Application.DAL;
using YetAnotherJira.Domain;
using YetAnotherJira.Domain.Entities;

namespace YetAnotherJira.Application.Mappers;

public static class UserMapper
{
    public static User Map(UserDal model)
        => model is null
            ? null
            : User.Create(
                model.Id,
                model.Username,
                model.Email,
                model.PasswordHash,
                model.CreatedAt,
                model.IsActive
            );

    public static UserDal Map(User model)
        => model is null
            ? null
            : new UserDal
            {
                Id = model.Id,
                Username = model.Username,
                Email = model.Email,
                PasswordHash = model.PasswordHash,
                CreatedAt = model.CreatedAt,
                IsActive = model.IsActive
            };
}
