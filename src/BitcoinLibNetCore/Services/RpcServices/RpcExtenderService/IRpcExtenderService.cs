// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System.Collections.Generic;
using BitcoinLib.Requests.CreateRawTransaction;
using BitcoinLib.Responses;
using System.Threading.Tasks;

namespace BitcoinLib.Services.RpcServices.RpcExtenderService
{
    public interface IRpcExtenderService
    {
       Task<decimal> GetAddressBalanceAsync(string inWalletAddress, int minConf = 0, bool validateAddressBeforeProcessing = true);
       Task<decimal> GetMinimumNonZeroTransactionFeeEstimateAsync(short numberOfInputs = 1, short numberOfOutputs = 1);
        Task<Dictionary<string, string>> GetMyPublicAndPrivateKeyPairsAsync();
        Task<DecodeRawTransactionResponse> GetPublicTransactionAsync(string txId);
       Task<decimal> GetTransactionFeeAsync(CreateRawTransactionRequest createRawTransactionRequest, bool checkIfTransactionQualifiesForFreeRelay = true, bool enforceMinimumTransactionFeePolicy = true);
       Task<decimal> GetTransactionPriorityAsync(CreateRawTransactionRequest createRawTransactionRequest);
       decimal GetTransactionPriority(IList<ListUnspentResponse> transactionInputs, int numberOfOutputs);
        Task<string> GetTransactionSenderAddressAsync(string txId);
        int GetTransactionSizeInBytes(CreateRawTransactionRequest createRawTransactionRequest);
        int GetTransactionSizeInBytes(int numberOfInputs, int numberOfOutputs);
        Task<GetRawTransactionResponse> GetRawTxFromImmutableTxId(string rigidTxId, int listTransactionsCount = int.MaxValue, int listTransactionsFrom = 0, bool getRawTransactionVersbose = true, bool rigidTxIdIsSha256 = false);
        Task<string> GetImmutableTxIdAsync(string txId, bool getSha256Hash = false);
       Task<bool> IsInWalletTransactionAsync(string txId);
       Task<bool> IsTransactionFreeAsync(CreateRawTransactionRequest createRawTransactionRequest);
       bool IsTransactionFree(IList<ListUnspentResponse> transactionInputs, int numberOfOutputs, decimal minimumAmountAmongOutputs);
       Task<bool> IsWalletEncryptedAsync();
    }
}