// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BitcoinLib.Auxiliary;
using BitcoinLib.ExceptionHandling.RpcExtenderService;
using BitcoinLib.ExtensionMethods;
using BitcoinLib.Requests.CreateRawTransaction;
using BitcoinLib.Responses;
using BitcoinLib.RPC.Specifications;
using BitcoinLib.Services.Coins.Base;
using System.Threading.Tasks;

namespace BitcoinLib.Services
{
    public partial class CoinService
    {
        //  Note: This will return funky results if the address in question along with its private key have been used to create a multisig address with unspent funds
        public async Task<decimal> GetAddressBalanceAsync(string inWalletAddress, int minConf, bool validateAddressBeforeProcessing)
        {
            if (validateAddressBeforeProcessing)
            {
                var validateAddressResponse = await ValidateAddressAsync(inWalletAddress);

                if (!validateAddressResponse.IsValid)
                {
                    throw new GetAddressBalanceException($"Address {inWalletAddress} is invalid!");
                }

                if (!validateAddressResponse.IsMine)
                {
                    throw new GetAddressBalanceException($"Address {inWalletAddress} is not an in-wallet address!");
                }
            }

            var listUnspentResponses = await ListUnspentAsync(minConf, 9999999, new List<string>
            {
                inWalletAddress
            });

            return listUnspentResponses.Any() ? listUnspentResponses.Sum(x => x.Amount) : 0;
        }

        public async Task<string> GetImmutableTxIdAsync(string txId, bool getSha256Hash)
        {
            var response = await GetRawTransactionAsync(txId, 1);
            var text = response.Vin.First().TxId + "|" + response.Vin.First().Vout + "|" + response.Vout.First().Value;
            return getSha256Hash ? Hashing.GetSha256(text) : text;
        }

        //  Get a rough estimate on fees for non-free txs, depending on the total number of tx inputs and outputs
        [Obsolete("Please don't use this method to calculate tx fees, its purpose is to provide a rough estimate only")]
        public async Task<decimal> GetMinimumNonZeroTransactionFeeEstimateAsync(short numberOfInputs = 1, short numberOfOutputs = 1)
        {
            var rawTransactionRequest = new CreateRawTransactionRequest(new List<CreateRawTransactionInput>(numberOfInputs), new Dictionary<string, decimal>(numberOfOutputs));

            for (short i = 0; i < numberOfInputs; i++)
            {
                rawTransactionRequest.AddInput(new CreateRawTransactionInput
                {
                    TxId = "dummyTxId" + i.ToString(CultureInfo.InvariantCulture),
                    Vout = i
                });
            }

            for (short i = 0; i < numberOfOutputs; i++)
            {
                rawTransactionRequest.AddOutput(new CreateRawTransactionOutput
                {
                    Address = "dummyAddress" + i.ToString(CultureInfo.InvariantCulture),
                    Amount = i + 1
                });
            }

            return await GetTransactionFeeAsync(rawTransactionRequest, false, true);
        }

        public async Task<Dictionary<string, string>> GetMyPublicAndPrivateKeyPairsAsync()
        {
            const short secondsToUnlockTheWallet = 30;
            var keyPairs = new Dictionary<string, string>();
            await WalletPassphraseAsync(Parameters.WalletPassword, secondsToUnlockTheWallet);
            var myAddresses = await (this as ICoinService).ListReceivedByAddressAsync(0, true);

            foreach (var listReceivedByAddressResponse in myAddresses)
            {
                var validateAddressResponse = await ValidateAddressAsync(listReceivedByAddressResponse.Address);

                if (validateAddressResponse.IsMine && validateAddressResponse.IsValid && !validateAddressResponse.IsScript)
                {
                    var privateKey = await DumpPrivKeyAsync(listReceivedByAddressResponse.Address);
                    keyPairs.Add(validateAddressResponse.PubKey, privateKey);
                }
            }

            await WalletLockAsync();
            return keyPairs;
        }

        //  Note: As RPC's gettransaction works only for in-wallet transactions this had to be extended so it will work for every single transaction.
        public async Task<DecodeRawTransactionResponse> GetPublicTransactionAsync(string txId)
        {
            var rawTransaction = await GetRawTransactionAsync(txId, 0);
            return await DecodeRawTransactionAsync(rawTransaction.Hex);
        }

        [Obsolete("Please use EstimateFee() instead. You can however keep on using this method until the network fully adjusts to the new rules on fee calculation")]
        public async Task<decimal> GetTransactionFeeAsync(CreateRawTransactionRequest transaction, bool checkIfTransactionQualifiesForFreeRelay, bool enforceMinimumTransactionFeePolicy)
        {
            if (checkIfTransactionQualifiesForFreeRelay && await IsTransactionFreeAsync(transaction))
            {
                return 0;
            }

            decimal transactionSizeInBytes = GetTransactionSizeInBytes(transaction);
            var transactionFee = ((transactionSizeInBytes / Parameters.FreeTransactionMaximumSizeInBytes) + (transactionSizeInBytes % Parameters.FreeTransactionMaximumSizeInBytes == 0 ? 0 : 1)) * Parameters.FeePerThousandBytesInCoins;

            if (transactionFee.GetNumberOfDecimalPlaces() > Parameters.CoinsPerBaseUnit.GetNumberOfDecimalPlaces())
            {
                transactionFee = decimal.Round(transactionFee, Parameters.CoinsPerBaseUnit.GetNumberOfDecimalPlaces(), MidpointRounding.AwayFromZero);
            }

            if (enforceMinimumTransactionFeePolicy && Parameters.MinimumTransactionFeeInCoins != 0 && transactionFee < Parameters.MinimumTransactionFeeInCoins)
            {
                transactionFee = Parameters.MinimumTransactionFeeInCoins;
            }

            return transactionFee;
        }

        public async Task<GetRawTransactionResponse> GetRawTxFromImmutableTxId(string rigidTxId, int listTransactionsCount, int listTransactionsFrom, bool getRawTransactionVersbose, bool rigidTxIdIsSha256)
        {
            var allTransactions = await (this as ICoinService).ListTransactionsAsync("*", listTransactionsCount, listTransactionsFrom);
            List<GetRawTransactionResponse> rawTrasactions = new List<GetRawTransactionResponse>();

            foreach (var listTransactionsResponse in allTransactions)
            {
                if (rigidTxId == await GetImmutableTxIdAsync(listTransactionsResponse.TxId, rigidTxIdIsSha256))
                {
                    rawTrasactions.Add(await GetRawTransactionAsync(listTransactionsResponse.TxId, getRawTransactionVersbose ? 1 : 0));
                }
            }

            return rawTrasactions.FirstOrDefault();

            //return allTransactions.Where(async x => await GetImmutableTxIdAsync(x.TxId, rigidTxIdIsSha256) == rigidTxId).Select(async x => await GetRawTransactionAsync(x.TxId, getRawTransactionVersbose ? 1 : 0)).FirstOrDefault();
            //return ( from listTransactionsResponse in allTransactions
            //  where rigidTxId == await GetImmutableTxIdAsync(listTransactionsResponse.TxId, rigidTxIdIsSha256)
            //select await GetRawTransactionAsync(listTransactionsResponse.TxId, getRawTransactionVersbose ? 1 : 0)).FirstOrDefault();
        }

        public async Task<decimal> GetTransactionPriorityAsync(CreateRawTransactionRequest transaction)
        {
            if (transaction.Inputs.Count == 0)
            {
                return 0;
            }

            var result = await (this as ICoinService).ListUnspentAsync(0);
            var unspentInputs = result.ToList();
            var sumOfInputsValueInBaseUnitsMultipliedByTheirAge = transaction.Inputs.Select(input => unspentInputs.First(x => x.TxId == input.TxId)).Select(unspentResponse => (unspentResponse.Amount * Parameters.OneCoinInBaseUnits) * unspentResponse.Confirmations).Sum();
            return sumOfInputsValueInBaseUnitsMultipliedByTheirAge / GetTransactionSizeInBytes(transaction);
        }

        public decimal GetTransactionPriority(IList<ListUnspentResponse> transactionInputs, int numberOfOutputs)
        {
            if (transactionInputs.Count == 0)
            {
                return 0;
            }

            return transactionInputs.Sum(input => input.Amount * Parameters.OneCoinInBaseUnits * input.Confirmations) / GetTransactionSizeInBytes(transactionInputs.Count, numberOfOutputs);
        }

        //  Note: Be careful when using GetTransactionSenderAddress() as it just gives you an address owned by someone who previously controlled the transaction's outputs
        //  which might not actually be the sender (e.g. for e-wallets) and who may not intend to receive anything there in the first place. 
        [Obsolete("Please don't use this method in production enviroment, it's for testing purposes only")]
        public async Task<string> GetTransactionSenderAddressAsync(string txId)
        {
            var rawTransaction = await GetRawTransactionAsync(txId, 0);
            var decodedRawTransaction = await DecodeRawTransactionAsync(rawTransaction.Hex);
            var transactionInputs = decodedRawTransaction.Vin;
            var rawTransactionHex = await GetRawTransactionAsync(transactionInputs[0].TxId, 0);
            var inputDecodedRawTransaction = await DecodeRawTransactionAsync(rawTransactionHex.Hex);
            var vouts = inputDecodedRawTransaction.Vout;
            return vouts[0].ScriptPubKey.Addresses[0];
        }

        public int GetTransactionSizeInBytes(CreateRawTransactionRequest transaction)
        {
            return GetTransactionSizeInBytes(transaction.Inputs.Count, transaction.Outputs.Count);
        }

        public int GetTransactionSizeInBytes(int numberOfInputs, int numberOfOutputs)
        {
            return numberOfInputs * Parameters.TransactionSizeBytesContributedByEachInput
                   + numberOfOutputs * Parameters.TransactionSizeBytesContributedByEachOutput
                   + Parameters.TransactionSizeFixedExtraSizeInBytes
                   + numberOfInputs;
        }

        public async Task<bool> IsInWalletTransactionAsync(string txId)
        {
            //  Note: This might not be efficient if iterated, consider caching ListTransactions' results.
            var result = await (this as ICoinService).ListTransactionsAsync(null, int.MaxValue);
            return result.Any(listTransactionsResponse => listTransactionsResponse.TxId == txId);
        }

        public async Task<bool> IsTransactionFreeAsync(CreateRawTransactionRequest transaction)
        {
            return transaction.Outputs.Any(x => x.Value < Parameters.FreeTransactionMinimumOutputAmountInCoins)
                   && GetTransactionSizeInBytes(transaction) < Parameters.FreeTransactionMaximumSizeInBytes
                   && await GetTransactionPriorityAsync(transaction) > Parameters.FreeTransactionMinimumPriority;
        }

        public bool IsTransactionFree(IList<ListUnspentResponse> transactionInputs, int numberOfOutputs, decimal minimumAmountAmongOutputs)
        {
            return minimumAmountAmongOutputs < Parameters.FreeTransactionMinimumOutputAmountInCoins
                   && GetTransactionSizeInBytes(transactionInputs.Count, numberOfOutputs) < Parameters.FreeTransactionMaximumSizeInBytes
                   && GetTransactionPriority(transactionInputs, numberOfOutputs) > Parameters.FreeTransactionMinimumPriority;
        }

        public async Task<bool> IsWalletEncryptedAsync()
        {
            var result = await HelpAsync(RpcMethods.walletlock.ToString());
            return !result.Contains("unknown command");
        }
    }
}