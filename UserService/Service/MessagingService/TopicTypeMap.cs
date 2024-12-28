using UserService.Model.Dto;

namespace UserService.Service.MessagingService;

public static class TopicTypeMap
{
    public static readonly Dictionary<KafkaTopic, Type> Map =
        new() { { KafkaTopic.UserCreated, typeof(UserDto) } };
}

public enum KafkaTopic
{
    UserCreated,
    AnotherTopic,
}
