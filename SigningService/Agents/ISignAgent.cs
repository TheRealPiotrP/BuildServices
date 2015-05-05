using System;
using System.Threading.Tasks;

namespace SigningService.Agents
{
    public interface ISignAgent
    {
        Task SignPackage(Uri packageUri);
    }
}