namespace UserService.Repository.Contract;

public interface IRepositoryManager
{
    public IUserRepository UserRepository { get; }
}
