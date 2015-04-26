using System;
using System.Threading.Tasks;

namespace MyGetConnector.Agents
{
    public interface IAddPackageAgent
    {
        Task AddPackage(Uri packageUrl);
    }
}