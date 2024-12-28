using UserService.Filters;
using UserService.Mapper;
using UserService.Mapper.UserMapper;
using UserService.Model.Dto;
using UserService.Model.Entity;
using UserService.Repository.Contract;
using UserService.Repository.Implementation;
using UserService.Service.Contract;
using UserService.Service.Implementation;

namespace UserService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Scoped services registration
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ValidationFilterAttribute>();

        // Mapper-related scoped services
        services.AddScoped<IBaseMapper<ApplicationUser, UserDto>, ApplicationUserToUserDto>();
        services.AddScoped<
            IBaseMapper<RegistrationRequestDto, ApplicationUser>,
            RegistrationsRequestDtoToApplicationUser
        >();
        services.AddScoped<IMapperManager, MapperManager>();

        // Repository-related scoped services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}
