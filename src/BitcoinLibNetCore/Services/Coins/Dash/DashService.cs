using BitcoinLib.CoinParameters.Dash;
using BitcoinLib.RPC.Specifications;
using System.Threading.Tasks;

namespace BitcoinLib.Services.Coins.Dash
{
    public class DashService : CoinService, IDashService
    {
        public DashService(bool useTestnet = false) : base(useTestnet)
        {
        }

        public DashService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword)
            : base(daemonUrl, rpcUsername, rpcPassword, walletPassword)
        {
        }

        public DashService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword,
            short rpcRequestTimeoutInSeconds)
            : base(daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds)
        {
        }

        /// <inheritdoc />
        public async Task<string> SendToAddressAsync(string dashAddress, decimal amount, string comment = null, string commentTo = null,
            bool subtractFeeFromAmount = false, bool useInstantSend = false, bool usePrivateSend = false)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.sendtoaddress, dashAddress, amount, comment, commentTo,
                subtractFeeFromAmount, useInstantSend, usePrivateSend);
        }

        public DashConstants.Constants Constants => DashConstants.Constants.Instance;
    }
}