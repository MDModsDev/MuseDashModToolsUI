namespace Euterpe.CodeAnalysis.Tests.Generators;

[TestSubject(typeof(EuterpeApiUrlGenerator))]
[Category("EuterpeApiUrlGeneratorTests")]
public sealed class EuterpeApiUrlGeneratorTest
{
    [Test]
    public void RunGenerators_EuterpeApi_GeneratesUrls()
    {
        const string source = """
                              namespace Euterpe.Shared
                              {
                                  public static partial class EuterpeApi
                                  {
                                      public const string BaseUrl = "https://example.com/api/";

                                      public static partial class Account
                                      {
                                          public const string BasePath = "me";
                                      }

                                      public static partial class Auth
                                      {
                                          public const string BasePath = "auth";
                                          public const string AppToken = "/app/token";
                                          public const string Logout = "/logout";
                                      }

                                      public static partial class Distribution
                                      {
                                          public const string BasePath = "distribution";
                                          public const string LibsPath = "/libs";
                                      }
                                  }
                              }
                              """;

        Snapshot.Validate(GeneratorTestHelper.Run<EuterpeApiUrlGenerator>(source));
    }
}
