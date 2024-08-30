using AuthService.Filters;
using AuthService.Mapper;
using AuthService.Mapper.UserMapper;
using AuthService.Model.Dto;
using AuthService.Model.Entity;
using AuthService.Repository.Contract;
using AuthService.Repository.Implementation;
using AuthService.Service.Contract;
using AuthService.Service.Implementation;

namespace AuthService.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        // Scoped services registration
        services.AddScoped<IAuthService, Service.Implementation.AuthService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ValidationFilterAttribute>();

        // Mapper-related scoped services
        services.AddScoped<IBaseMapper<ApplicationUser, UserDto>, ApplicationUserToUserDto>();
        services.AddScoped<IBaseMapper<RegistrationRequestDto, ApplicationUser>, RegistrationsRequestDtoToApplicationUser>();
        services.AddScoped<IMapperManager, MapperManager>();

        // Repository-related scoped services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        return services;
    }
}