using AuthService.Model;
using AuthService.Repository.Contract;

namespace AuthService.Repository.Implementation;

public class RepositoryManager(IUserRepository userRepository) : IRepositoryManager
{
    public IUserRepository UserRepository { get; } = userRepository;
}