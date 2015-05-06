using System;
using System.Linq;
using FluentAssertions;
using Its.Configuration;
using Microsoft.Its.Recipes;
using SigningService.Agents;
using Xunit;

namespace SigningService.Tests
{
    public class KeyVaultAgentSpecs
    {
        [Fact]
        public async void When_digest_has_32_bytes_the_response_has_256_bytes()
        {
            Settings.Precedence = new[] {"test"};
            
            var keyVaultAgent = new KeyVaultAgent();

            var response = await keyVaultAgent.Sign(Any.Sequence(x => Any.Byte(), 32).ToArray());

            response
                .Should().HaveCount(256, "Because that is the length of an RSA256 signed digest");
        }

        [Fact]
        public async void When_digest_has_more_or_less_than_32_bytes_Then_it_fails_with_a_useful_message()
        {
            var keyVaultAgent = new KeyVaultAgent();

            var byteCount = Any.Int(0, 1024);
            if (byteCount == 32) byteCount += Any.Int(1, 1024);

            Action sign = () => { keyVaultAgent.Sign(new byte[byteCount]).Wait(); };

            sign
                .ShouldThrow<ArgumentException>("Because only 32 bit digests are accepted by RSA256")
                .WithMessage("The value must have 32 bytes\r\nParameter name: digest");
        }

        [Fact]
        public async void When_digest_is_null_Then_it_fails_with_a_useful_message()
        {
            var keyVaultAgent = new KeyVaultAgent();
            
            Action sign = () => keyVaultAgent.Sign(null).Wait();

            sign
                .ShouldThrow<ArgumentNullException>("Because a digest must be provided.")
                .WithMessage("Value cannot be null.\r\nParameter name: digest");
        }
    }
}
