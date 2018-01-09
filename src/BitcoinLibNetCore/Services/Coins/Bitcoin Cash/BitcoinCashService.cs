using BitcoinLib.CoinParameters.BitcoinCash;
using BitcoinLib.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinLibNetCore.Services.Coins.Bitcoin_Cash
{
    public class BitcoinCashService : CoinService, IBitcoinCashService
    {
        public BitcoinCashService(bool useTestnet = false) : base(useTestnet)
        {
        }

        public BitcoinCashService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword)
            : base(daemonUrl, rpcUsername, rpcPassword, walletPassword)
        {
        }

        public BitcoinCashService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword, short rpcRequestTimeoutInSeconds)
            : base(daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds)
        {
        }

        public BitcoinCashService(bool useTestNet, string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword, short rpcRequestTimeoutInSeconds)
    : base(useTestNet, daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds)
        {
        }

        public BitcoinCashConstants.Constants Constants => BitcoinCashConstants.Constants.Instance;
    }
}
