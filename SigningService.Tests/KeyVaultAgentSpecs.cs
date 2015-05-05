using Its.Configuration;
using Microsoft.Its.Recipes;
using SigningService.Agents;
using Xunit;

namespace SigningService.Tests
{
    public class KeyVaultAgentSpecs
    {
        [Fact]
        public async void When_called_it_returns()
        {
            Settings.Precedence = new[] {"test"};

            var keyVaultAgent = new KeyVaultAgent();

            var foo = await keyVaultAgent.Sign(Any.String(32,32, Characters.LatinLettersAndNumbers()));
        }
    }
}
