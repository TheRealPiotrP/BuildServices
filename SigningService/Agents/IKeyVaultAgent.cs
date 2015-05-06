using System.Threading.Tasks;

namespace SigningService.Agents
{
    public interface IKeyVaultAgent
    {
        Task<byte[]> Sign(byte[] digest);
    }
}