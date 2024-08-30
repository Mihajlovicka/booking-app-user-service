using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Model.Dto;

namespace AuthService.Service.MessagingService;
public static class TopicTypeMap
{
    public static readonly Dictionary<KafkaTopic, Type> Map = new()
    {
        { KafkaTopic.TestTopic, typeof(LoginResponseDto) }
    };
}

public enum KafkaTopic
{
    TestTopic,
    AnotherTopic
}
