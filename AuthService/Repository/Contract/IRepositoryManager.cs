namespace AuthService.Repository.Contract;

public interface IRepositoryManager
{
    public IUserRepository UserRepository { get; }
}