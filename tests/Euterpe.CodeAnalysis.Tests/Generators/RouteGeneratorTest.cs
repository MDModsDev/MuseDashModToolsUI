namespace Euterpe.CodeAnalysis.Tests.Generators;

[TestSubject(typeof(RouteGenerator))]
[Category("RouteGeneratorTests")]
public sealed class RouteGeneratorTest
{
    [Test]
    public void RunGenerators_NestedRoutes_GeneratesRouteTree()
    {
        const string source = """
                              namespace Sample
                              {
                                  using Euterpe.Shared.Attributes;

                                  [Route("/", DisplayName = "Root", Order = 0)]
                                  public partial class RootViewModel;

                                  [Route("/home", DisplayName = "Home", Icon = "house", Order = 0)]
                                  public partial class HomeViewModel;

                                  [Route("/settings", DisplayName = "Settings", Icon = "gear", Order = 1)]
                                  [AppSingleton]
                                  public partial class SettingsViewModel;

                                  [Route("/settings/general", DisplayName = "General", Order = 0)]
                                  [AppSingleton]
                                  public partial class GeneralViewModel;

                                  [Route("/settings/advanced", DisplayName = "Advanced", Order = 1)]
                                  public partial class AdvancedViewModel;
                              }

                              namespace Euterpe.Shared.Attributes
                              {
                                  [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                  public sealed class RouteAttribute(string path) : System.Attribute
                                  {
                                      public string Path { get; } = path;
                                      public string DisplayName { get; init; } = "";
                                      public string Icon { get; init; } = "";
                                      public int Order { get; init; }
                                  }

                                  [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                  public sealed class AppSingletonAttribute : System.Attribute;
                              }
                              """;

        Snapshot.Validate(GeneratorTestHelper.Run<RouteGenerator>(source));
    }
}
