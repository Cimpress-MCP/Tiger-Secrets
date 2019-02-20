using System.Linq;
using System.Threading;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Test
{
    /// <summary>Tests of actually getting secrets.</summary>
    [Properties(QuietOnSuccess = true)]
    public static class RetrievalTests
    {
        [Property(DisplayName = "If no secret is configured for the specified ID, that's OK.")]
        public static void NoConfiguredSecret_OK(string message, NonEmptyString secretId)
        {
            var client = new Mock<IAmazonSecretsManager>();
            client.Setup(m => m.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ResourceNotFoundException(message));

            var sut = new AWSSecretsManagerConfigurationProvider(new AWSSecretsManagerConfigurationSource(client.Object, secretId.Get));
            sut.Load();

            client.Verify(m => m.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Empty(sut.GetChildKeys(Enumerable.Empty<string>(), null));
        }
    }
}
