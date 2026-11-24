using AutoMapper;
using NLayerShop.Business.Mapping;

namespace NLayerShop.UnitTests.Infrastructure;

/// <summary>The production AutoMapper profile, built once per test run.</summary>
public static class TestMapper
{
    private static readonly Lazy<IMapper> Instance = new(() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper());

    public static IMapper Mapper => Instance.Value;
}
