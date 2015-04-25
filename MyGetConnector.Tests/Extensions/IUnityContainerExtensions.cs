using Microsoft.Practices.Unity;
using Moq;

namespace MyGetConnector.Tests.Extensions
{
    public static class IUnityContainerExtensions
    {
        public static Mock<T> GetMock<T>(this IUnityContainer container) where T : class
        {
            return Mock.Get(container.Resolve<T>());
        }
    }
}
