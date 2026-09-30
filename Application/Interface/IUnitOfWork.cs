using DataAccess.Repo;

namespace Application.Interface
{
    public interface IUnitOfWork
    {
        UserRepo userRepo { get; }

        Task<int> CompleteAsync();
    }
}