using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace SigningService.Tests.Utils
{
    public class TestData
    {
        public static IEnumerable<object[]> AllTestAssemblies()
        {
            foreach (var assembly in TestAssembly.GetTestAssemblies())
            {
                yield return new object[] { assembly };
            }
        }

        public static IEnumerable<object[]> TestAssembliesWithKnownHash()
        {
            foreach (var assembly in TestAssembly.GetTestAssemblies())
            {
                if (assembly.HasKnownHash)
                {
                    yield return new object[] { assembly };
                }
            }
        }

        public static TestAssembly GetJScript()
        {
            foreach (var assembly in TestAssembly.GetTestAssemblies())
            {
                if (assembly.ResourceName == "Microsoft.JScript.dll")
                {
                    return assembly;
                }
            }

            true.Should().BeFalse("Microsoft.JScript.dll missing");
            return new TestAssembly();
        }
    }
}
