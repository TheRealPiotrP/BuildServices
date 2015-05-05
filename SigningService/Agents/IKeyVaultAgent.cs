using System.Threading.Tasks;

namespace SigningService.Agents
{
    public interface IKeyVaultAgent
    {
        Task<string> Sign(string digest);
    }
}