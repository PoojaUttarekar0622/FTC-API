using System.Threading.Tasks;

namespace Helper.Interface
{
    public interface ITypedHubClient
    {
        #region
        Task BroadcastMessage();
        #endregion
    }
}