using BitcoinLib.CoinParameters.Bitcoin;
using BitcoinLib.CoinParameters.BitcoinCash;
using BitcoinLib.Services.Coins.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinLibNetCore.Services.Coins.Bitcoin_Cash
{
    public interface IBitcoinCashService : ICoinService, IBitcoinCashConstants
    {
    }
}
