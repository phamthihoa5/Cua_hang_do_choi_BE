using Application.Interface;
using DataAccess;
using DataAccess.Repo;

namespace Application.AppService
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;

        private UserRepo? _userRepo;
        private NewRepo? _newRepo;
        private PromotionRepo? _promotionRepo;
        private ProductRepo? _productRepo;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }

        public UserRepo userRepo =>
            _userRepo ??= new UserRepo(_context);

        public NewRepo newRepo =>
            _newRepo ??= new NewRepo(_context);

        public PromotionRepo promotionRepo =>
            _promotionRepo ??= new PromotionRepo(_context);

        public ProductRepo productRepo =>
            _productRepo ??= new ProductRepo(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}