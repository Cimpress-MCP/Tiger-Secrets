using System.Threading;
using Amazon.SecretsManager;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Test
{
    /// <summary>Tests of reloading secrets.</summary>
    [Properties(QuietOnSuccess = true)]
    public static class ReloadTests
    {
        [Property(DisplayName = "If no reload is occurring, the wait should be effectively immediate.")]
        public static void NoReload_OK(NonEmptyString secretId)
        {
            var client = Mock.Of<IAmazonSecretsManager>();

            var configurationSource = new SecretsManagerConfigurationSource(client, secretId.Get, Timeout.InfiniteTimeSpan);
            var sut = new SecretsManagerConfigurationProvider(configurationSource);

            // note(cosborn) Assertion controlled by the "longRunningTestSeconds" parameter in `xunit.runner.json`.
            sut.WaitForReloadToComplete(Timeout.InfiniteTimeSpan);
        }
    }
}
