using System.Threading;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using static System.StringComparer;

namespace Test
{
    /// <summary>Tests of actually getting secrets.</summary>
    [Properties(QuietOnSuccess = true)]
    public static class RetrievalTests
    {
        [Property(DisplayName = "If no secret is configured for the specified ID, that's bad.")]
        public static void NoConfiguredSecret_Exception(NonEmptyString message, NonEmptyString secretId)
        {
            var client = new Mock<IAmazonSecretsManager>();
            client.Setup(m => m.GetSecretValueAsync(It.IsNotNull<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException(message.Get));

            var configurationSource = new SecretsManagerConfigurationSource(client.Object, secretId.Get, Timeout.InfiniteTimeSpan);
            var sut = new SecretsManagerConfigurationProvider(configurationSource);
            var actual = Record.Exception(sut.Load);

            client.Verify(
                m => m.GetSecretValueAsync(
                    It.Is<GetSecretValueRequest>(r => r.SecretId == secretId.Get),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.NotNull(actual);
            var rnfe = Assert.IsAssignableFrom<ResourceNotFoundException>(actual);
            Assert.Equal(message.Get, rnfe.Message, Ordinal);
        }
    }
}
