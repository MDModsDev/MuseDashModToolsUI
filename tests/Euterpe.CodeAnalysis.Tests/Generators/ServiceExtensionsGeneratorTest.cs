namespace Euterpe.CodeAnalysis.Tests.Generators;

[TestSubject(typeof(ServiceExtensionsGenerator))]
[Category("ServiceExtensionsGeneratorTests")]
public sealed class ServiceExtensionsGeneratorTest
{
    [Test]
    public void RunGenerators_AppAndPerGameViewModels_GeneratesRegistrations()
    {
        const string source = """
                              namespace Sample
                              {
                                  using Euterpe.Shared.Attributes;

                                  [Route("/")]
                                  [AppSingleton]
                                  public partial class RootViewModel;

                                  [Route("/home")]
                                  [AppSingleton]
                                  public partial class HomeViewModel;

                                  [Route("/modding")]
                                  public partial class ModdingViewModel;

                                  [Register]
                                  public partial class WizardDialogViewModel;

                                  [Register]
                                  [AppSingleton]
                                  public partial class CrashViewModel;
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

                                  [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
                                  public sealed class RegisterAttribute : System.Attribute;
                              }
                              """;

        Snapshot.Validate(GeneratorTestHelper.Run<ServiceExtensionsGenerator>(source));
    }
}
