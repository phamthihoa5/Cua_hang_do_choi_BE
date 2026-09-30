using Application.Interface;
using DataAccess;
using DataAccess.Repo;

namespace Application.AppService
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;
        private UserRepo? _userRepo;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }

        public UserRepo userRepo => _userRepo ??= new UserRepo(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}