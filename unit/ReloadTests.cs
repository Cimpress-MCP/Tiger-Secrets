// <copyright file="ReloadTests.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

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
            using var sut = new SecretsManagerConfigurationProvider(configurationSource);

            // note(cosborn) Assertion controlled by the "longRunningTestSeconds" parameter in `xunit.runner.json`.
            sut.WaitForReloadToComplete(Timeout.InfiniteTimeSpan);
        }
    }
}
