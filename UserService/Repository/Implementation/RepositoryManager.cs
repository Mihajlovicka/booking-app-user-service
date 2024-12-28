using UserService.Repository.Contract;

namespace UserService.Repository.Implementation;

public class RepositoryManager(IUserRepository userRepository) : IRepositoryManager
{
    public IUserRepository UserRepository { get; } = userRepository;
}
