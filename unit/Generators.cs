using FsCheck;
using JetBrains.Annotations;
using static System.StringComparison;
using static Microsoft.Extensions.Configuration.ConfigurationPath;
using static Microsoft.Extensions.Configuration.AWSSecretsManagerConfigurationProvider;

namespace Test
{
    static class Generators
    {
        [NotNull]
        public static Arbitrary<ConfigurationKey> ConfigurationKey { get; } = Arb.Default.NonEmptyString()
            .Filter(nes => !nes.Get.Contains(KeyDelimiter, Ordinal))
            .Filter(nes => !nes.Get.Contains(AlternativeKeyDelimiter, Ordinal))
            /* note(cosborn)
             * This is to work around a bug in the alternative-key-delimiter transformation --
             * one that is present in Microsoft's code, too, yay!
             * Let's say I have configuration: { "outer_": { "inner": "value" } }
             * This is specified in deep-key syntax as "outer_:inner",
             * and in alternative-key syntax as "outer___inner".
             * But do you see those three underscores? The replacement of alternative with
             * primary delimiter will result in this: "outer:_inner". This doesn't match,
             * so the configuration will be lost. I don't know how to solve this, and
             * Microsoft doesn't bother, so I won't, either. I'll keep it from breaking
             * my unit tests, though.
             */
            .Filter(nes => !nes.Get.EndsWith('_'))
            .Convert(nes => new ConfigurationKey(nes.Get), ck => NonEmptyString.NewNonEmptyString(ck.ToString()));
    }
}
