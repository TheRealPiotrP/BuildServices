using System.Reflection;
using System.Threading.Tasks;

namespace SigningService.Agents
{
    public interface IKeyVaultAgent
    {
        Task<byte[]> SignAsync(string keyId, byte[] digest);
        Task<string> GetRsaKeyIdAsync(byte[] exponent, byte[] modulus);
    }
}