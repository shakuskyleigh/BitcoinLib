// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using System;
using System.Collections.Generic;
using System.Linq;
using BitcoinLib.Requests.AddNode;
using BitcoinLib.Requests.CreateRawTransaction;
using BitcoinLib.Requests.SignRawTransaction;
using BitcoinLib.Responses;
using BitcoinLib.RPC.Connector;
using BitcoinLib.RPC.Specifications;
using BitcoinLib.Services.Coins.Base;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace BitcoinLib.Services
{
    //   Implementation of API calls list, as found at: https://en.bitcoin.it/wiki/Original_Bitcoin_client/API_Calls_list (note: this list is often out-of-date so call "help" in your bitcoin-cli to get the latest signatures)
    public partial class CoinService : ICoinService
    {
        protected readonly IRpcConnector _rpcConnector;

        public CoinService()
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, null, null, null, null, 0);
        }

        public CoinService(bool useTestnet) : this()
        {
            Parameters.UseTestnet = useTestnet;
        }

        public CoinService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword)
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, daemonUrl, rpcUsername, rpcPassword, walletPassword, 0);
        }

        //  this provides support for cases where *.config files are not an option
        public CoinService(string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword, short rpcRequestTimeoutInSeconds)
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds);
        }

        public CoinService(bool useTestNet, string daemonUrl, string rpcUsername, string rpcPassword, string walletPassword, short rpcRequestTimeoutInSeconds)
        {
            _rpcConnector = new RpcConnector(this);
            Parameters = new CoinParameters(this, daemonUrl, rpcUsername, rpcPassword, walletPassword, rpcRequestTimeoutInSeconds);
            Parameters.DaemonUrlTestnet = daemonUrl;
            Parameters.UseTestnet = useTestNet;
        }

        public async Task<string> AddMultiSigAddressAsync(int nRquired, List<string> publicKeys, string account)
        {
            return account != null
                ? await _rpcConnector.MakeRequest<string>(RpcMethods.addmultisigaddress, nRquired, publicKeys, account)
                : await _rpcConnector.MakeRequest<string>(RpcMethods.addmultisigaddress, nRquired, publicKeys);
        }

        public async Task AddNodeAsync(string node, NodeAction action)
        {
            await _rpcConnector.MakeRequest<string>(RpcMethods.addnode, node, action.ToString());
        }

        public async Task BackupWalletAsync(string destination)
        {
            await _rpcConnector.MakeRequest<string>(RpcMethods.backupwallet, destination);
        }

        public async Task<CreateMultiSigResponse> CreateMultiSigAsync(int nRquired, List<string> publicKeys)
        {
            return await _rpcConnector.MakeRequest<CreateMultiSigResponse>(RpcMethods.createmultisig, nRquired, publicKeys);
        }

        public async Task<string> CreateRawTransactionAsync(CreateRawTransactionRequest rawTransaction)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.createrawtransaction, rawTransaction.Inputs, rawTransaction.Outputs);
        }

        public async Task<DecodeRawTransactionResponse> DecodeRawTransactionAsync(string rawTransactionHexString)
        {
            return await _rpcConnector.MakeRequest<DecodeRawTransactionResponse>(RpcMethods.decoderawtransaction, rawTransactionHexString);
        }

        public async Task<DecodeScriptResponse> DecodeScriptAsync(string hexString)
        {
            return await _rpcConnector.MakeRequest<DecodeScriptResponse>(RpcMethods.decodescript, hexString);
        }

        public async Task<string> DumpPrivKeyAsync(string bitcoinAddress)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.dumpprivkey, bitcoinAddress);
        }

        public async Task DumpWalletAsync(string filename)
        {
            await _rpcConnector.MakeRequest<string>(RpcMethods.dumpwallet, filename);
        }

        public async Task<decimal> EstimateFeeAsync(ushort nBlocks)
        {
            return await _rpcConnector.MakeRequest<decimal>(RpcMethods.estimatefee, nBlocks);
        }

        public async Task<decimal> EstimatePriorityAsync(ushort nBlocks)
        {
            return await _rpcConnector.MakeRequest<decimal>(RpcMethods.estimatepriority, nBlocks);
        }

        public async Task<string> GetAccountAsync(string bitcoinAddress)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.getaccount, bitcoinAddress);
        }

        public async Task<string> GetAccountAddressAsync(string account)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.getaccountaddress, account);
        }

        public async Task<GetAddedNodeInfoResponse> GetAddedNodeInfoAsync(string dns, string node)
        {
            return string.IsNullOrWhiteSpace(node)
                ? await _rpcConnector.MakeRequest<GetAddedNodeInfoResponse>(RpcMethods.getaddednodeinfo, dns)
                : await _rpcConnector.MakeRequest<GetAddedNodeInfoResponse>(RpcMethods.getaddednodeinfo, dns, node);
        }

        public async Task<List<string>> GetAddressesByAccountAsync(string account)
        {
            return await _rpcConnector.MakeRequest<List<string>>(RpcMethods.getaddressesbyaccount, account);
        }

        public async Task<decimal> GetBalanceAsync(string account, int minConf, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<decimal>(RpcMethods.getbalance, (string.IsNullOrWhiteSpace(account) ? "*" : account), minConf)
                : await _rpcConnector.MakeRequest<decimal>(RpcMethods.getbalance, (string.IsNullOrWhiteSpace(account) ? "*" : account), minConf, includeWatchonly);
        }

        public async Task<string> GetBestBlockHashAsync()
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.getbestblockhash);
        }

        public async Task<GetBlockResponse> GetBlockAsync(string hash, bool verbose)
        {
            return await _rpcConnector.MakeRequest<GetBlockResponse>(RpcMethods.getblock, hash, verbose);
        }

        public async Task<GetBlockchainInfoResponse> GetBlockchainInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetBlockchainInfoResponse>(RpcMethods.getblockchaininfo);
        }

        public async Task<uint> GetBlockCountAsync()
        {
            return await _rpcConnector.MakeRequest<uint>(RpcMethods.getblockcount);
        }

        public async Task<string> GetBlockHashAsync(long index)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.getblockhash, index);
        }

        public async Task<GetBlockTemplateResponse> GetBlockTemplateAsync(params object[] parameters)
        {
            return parameters == null
                ? await _rpcConnector.MakeRequest<GetBlockTemplateResponse>(RpcMethods.getblocktemplate)
                : await _rpcConnector.MakeRequest<GetBlockTemplateResponse>(RpcMethods.getblocktemplate, parameters);
        }

        public async Task<List<GetChainTipsResponse>> GetChainTipsAsync()
        {
            return await _rpcConnector.MakeRequest<List<GetChainTipsResponse>>(RpcMethods.getchaintips);
        }

        public async Task<int> GetConnectionCountAsync()
        {
            return await _rpcConnector.MakeRequest<int>(RpcMethods.getconnectioncount);
        }

        public async Task<double> GetDifficultyAsync()
        {
            return await _rpcConnector.MakeRequest<double>(RpcMethods.getdifficulty);
        }

        public async Task<bool> GetGenerateAsync()
        {
            return await _rpcConnector.MakeRequest<bool>(RpcMethods.getgenerate);
        }

        [Obsolete("Please use calls: GetWalletInfo(), GetBlockchainInfo() and GetNetworkInfo() instead")]
        public async Task<GetInfoResponse> GetInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetInfoResponse>(RpcMethods.getinfo);
        }

        public async Task<GetMemPoolInfoResponse> GetMemPoolInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetMemPoolInfoResponse>(RpcMethods.getmempoolinfo);
        }

        public async Task<GetMiningInfoResponse> GetMiningInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetMiningInfoResponse>(RpcMethods.getmininginfo);
        }

        public async Task<GetNetTotalsResponse> GetNetTotalsAsync()
        {
            return await _rpcConnector.MakeRequest<GetNetTotalsResponse>(RpcMethods.getnettotals);
        }

        public async Task<ulong> GetNetworkHashPsAsync(uint blocks, long height)
        {
            return await _rpcConnector.MakeRequest<ulong>(RpcMethods.getnetworkhashps);
        }

        public async Task<GetNetworkInfoResponse> GetNetworkInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetNetworkInfoResponse>(RpcMethods.getnetworkinfo);
        }

        public async Task<string> GetNewAddressAsync(string account)
        {
            return string.IsNullOrWhiteSpace(account)
                ? await _rpcConnector.MakeRequest<string>(RpcMethods.getnewaddress)
                : await _rpcConnector.MakeRequest<string>(RpcMethods.getnewaddress, account);
        }

        public async Task<List<GetPeerInfoResponse>> GetPeerInfoAsync()
        {
            return await _rpcConnector.MakeRequest<List<GetPeerInfoResponse>>(RpcMethods.getpeerinfo);
        }

        public async Task<GetRawMemPoolResponse> GetRawMemPoolAsync(bool verbose)
        {
            var getRawMemPoolResponse = new GetRawMemPoolResponse
            {
                IsVerbose = verbose
            };

            var rpcResponse = await _rpcConnector.MakeRequest<object>(RpcMethods.getrawmempool, verbose);

            if (!verbose)
            {
                var rpcResponseAsArray = (JArray)rpcResponse;

                foreach (string txId in rpcResponseAsArray)
                {
                    getRawMemPoolResponse.TxIds.Add(txId);
                }

                return getRawMemPoolResponse;
            }

            IList<KeyValuePair<string, JToken>> rpcResponseAsKvp = (new EnumerableQuery<KeyValuePair<string, JToken>>(((JObject)(rpcResponse)))).ToList();
            IList<JToken> children = JObject.Parse(rpcResponse.ToString()).Children().ToList();

            for (var i = 0; i < children.Count(); i++)
            {
                var getRawMemPoolVerboseResponse = new GetRawMemPoolVerboseResponse
                {
                    TxId = rpcResponseAsKvp[i].Key
                };

                getRawMemPoolResponse.TxIds.Add(getRawMemPoolVerboseResponse.TxId);

                foreach (var property in children[i].SelectMany(grandChild => grandChild.OfType<JProperty>()))
                {
                    switch (property.Name)
                    {
                        case "currentpriority":

                            double currentPriority;

                            if (double.TryParse(property.Value.ToString(), out currentPriority))
                            {
                                getRawMemPoolVerboseResponse.CurrentPriority = currentPriority;
                            }

                            break;

                        case "depends":

                            foreach (var jToken in property.Value)
                            {
                                getRawMemPoolVerboseResponse.Depends.Add(jToken.Value<string>());
                            }

                            break;

                        case "fee":

                            decimal fee;

                            if (decimal.TryParse(property.Value.ToString(), out fee))
                            {
                                getRawMemPoolVerboseResponse.Fee = fee;
                            }

                            break;

                        case "height":

                            int height;

                            if (int.TryParse(property.Value.ToString(), out height))
                            {
                                getRawMemPoolVerboseResponse.Height = height;
                            }

                            break;

                        case "size":

                            int size;

                            if (int.TryParse(property.Value.ToString(), out size))
                            {
                                getRawMemPoolVerboseResponse.Size = size;
                            }

                            break;

                        case "startingpriority":

                            double startingPriority;

                            if (double.TryParse(property.Value.ToString(), out startingPriority))
                            {
                                getRawMemPoolVerboseResponse.StartingPriority = startingPriority;
                            }

                            break;

                        case "time":

                            int time;

                            if (int.TryParse(property.Value.ToString(), out time))
                            {
                                getRawMemPoolVerboseResponse.Time = time;
                            }

                            break;

                        default:

                            throw new Exception("Unkown property: " + property.Name + " in GetRawMemPool()");
                    }
                }
                getRawMemPoolResponse.VerboseResponses.Add(getRawMemPoolVerboseResponse);
            }
            return getRawMemPoolResponse;
        }

        public async Task<string> GetRawChangeAddressAsync()
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.getrawchangeaddress);
        }

        public async Task<GetRawTransactionResponse> GetRawTransactionAsync(string txId, int verbose)
        {
            if (verbose == 0)
            {
                return new GetRawTransactionResponse
                {
                    Hex = await _rpcConnector.MakeRequest<string>(RpcMethods.getrawtransaction, txId, verbose)
                };
            }

            if (verbose == 1)
            {
                return await _rpcConnector.MakeRequest<GetRawTransactionResponse>(RpcMethods.getrawtransaction, txId, verbose);
            }

            throw new Exception("Invalid verbose value: " + verbose + " in GetRawTransaction()!");
        }

        public async Task<decimal> GetReceivedByAccountAsync(string account, int minConf)
        {
            return await _rpcConnector.MakeRequest<decimal>(RpcMethods.getreceivedbyaccount, account, minConf);
        }

        public async Task<decimal> GetReceivedByAddressAsync(string bitcoinAddress, int minConf)
        {
            return await _rpcConnector.MakeRequest<decimal>(RpcMethods.getreceivedbyaddress, bitcoinAddress, minConf);
        }

        public async Task<GetTransactionResponse> GetTransactionAsync(string txId, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<GetTransactionResponse>(RpcMethods.gettransaction, txId)
                : await _rpcConnector.MakeRequest<GetTransactionResponse>(RpcMethods.gettransaction, txId, includeWatchonly);
        }

        public async Task<GetTransactionResponse> GetTxOutAsync(string txId, int n, bool includeMemPool)
        {
            return await _rpcConnector.MakeRequest<GetTransactionResponse>(RpcMethods.gettxout, txId, n, includeMemPool);
        }

        public async Task<GetTxOutSetInfoResponse> GetTxOutSetInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetTxOutSetInfoResponse>(RpcMethods.gettxoutsetinfo);
        }

        public async Task<decimal> GetUnconfirmedBalanceAsync()
        {
            return await _rpcConnector.MakeRequest<decimal>(RpcMethods.getunconfirmedbalance);
        }

        public async Task<GetWalletInfoResponse> GetWalletInfoAsync()
        {
            return await _rpcConnector.MakeRequest<GetWalletInfoResponse>(RpcMethods.getwalletinfo);
        }

        public async Task<string> HelpAsync(string command)
        {
            return string.IsNullOrWhiteSpace(command)
                ? await _rpcConnector.MakeRequest<string>(RpcMethods.help)
                : await _rpcConnector.MakeRequest<string>(RpcMethods.help, command);
        }

        public async Task ImportAddressAsync(string address, string label, bool rescan)
        {
            await _rpcConnector.MakeRequest<string>(RpcMethods.importaddress, address, label, rescan);
        }

        public async Task<string> ImportPrivKeyAsync(string privateKey, string label, bool rescan)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.importprivkey, privateKey, label, rescan);
        }

        public async Task ImportWalletAsync(string filename)
        {
            await _rpcConnector.MakeRequest<string>(RpcMethods.importwallet, filename);
        }

        public async Task<string> KeyPoolRefillAsync(uint newSize)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.keypoolrefill, newSize);
        }

        public async Task<Dictionary<string, decimal>> ListAccountsAsync(int minConf, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<Dictionary<string, decimal>>(RpcMethods.listaccounts, minConf)
                : await _rpcConnector.MakeRequest<Dictionary<string, decimal>>(RpcMethods.listaccounts, minConf, includeWatchonly);
        }

        public async Task<List<List<ListAddressGroupingsResponse>>> ListAddressGroupingsAsync()
        {
            var unstructuredResponse = await _rpcConnector.MakeRequest<List<List<List<object>>>>(RpcMethods.listaddressgroupings);
            var structuredResponse = new List<List<ListAddressGroupingsResponse>>(unstructuredResponse.Count);

            for (var i = 0; i < unstructuredResponse.Count; i++)
            {
                for (var j = 0; j < unstructuredResponse[i].Count; j++)
                {
                    if (unstructuredResponse[i][j].Count > 1)
                    {
                        var response = new ListAddressGroupingsResponse
                        {
                            Address = unstructuredResponse[i][j][0].ToString()
                        };

                        decimal.TryParse(unstructuredResponse[i][j][1].ToString(), out decimal balance);

                        if (unstructuredResponse[i][j].Count > 2)
                        {
                            response.Account = unstructuredResponse[i][j][2].ToString();
                        }

                        if (structuredResponse.Count < i + 1)
                        {
                            structuredResponse.Add(new List<ListAddressGroupingsResponse>());
                        }

                        structuredResponse[i].Add(response);
                    }
                }
            }
            return structuredResponse;
        }

        public async Task<string> ListLockUnspentAsync()
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.listlockunspent);
        }

        public async Task<List<ListReceivedByAccountResponse>> ListReceivedByAccountAsync(int minConf, bool includeEmpty, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<List<ListReceivedByAccountResponse>>(RpcMethods.listreceivedbyaccount, minConf, includeEmpty)
                : await _rpcConnector.MakeRequest<List<ListReceivedByAccountResponse>>(RpcMethods.listreceivedbyaccount, minConf, includeEmpty, includeWatchonly);
        }

        public async Task<List<ListReceivedByAddressResponse>> ListReceivedByAddressAsync(int minConf, bool includeEmpty, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<List<ListReceivedByAddressResponse>>(RpcMethods.listreceivedbyaddress, minConf, includeEmpty)
                : await _rpcConnector.MakeRequest<List<ListReceivedByAddressResponse>>(RpcMethods.listreceivedbyaddress, minConf, includeEmpty, includeWatchonly);
        }

        public async Task<ListSinceBlockResponse> ListSinceBlockAsync(string blockHash, int targetConfirmations, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<ListSinceBlockResponse>(RpcMethods.listsinceblock, (string.IsNullOrWhiteSpace(blockHash) ? "*" : blockHash), targetConfirmations)
                : await _rpcConnector.MakeRequest<ListSinceBlockResponse>(RpcMethods.listsinceblock, (string.IsNullOrWhiteSpace(blockHash) ? "*" : blockHash), targetConfirmations, includeWatchonly);
        }

        public async Task<List<ListTransactionsResponse>> ListTransactionsAsync(string account, int count, int from, bool? includeWatchonly)
        {
            return includeWatchonly == null
                ? await _rpcConnector.MakeRequest<List<ListTransactionsResponse>>(RpcMethods.listtransactions, (string.IsNullOrWhiteSpace(account) ? "*" : account), count, from)
                : await _rpcConnector.MakeRequest<List<ListTransactionsResponse>>(RpcMethods.listtransactions, (string.IsNullOrWhiteSpace(account) ? "*" : account), count, from, includeWatchonly);
        }

        public async Task<List<ListUnspentResponse>> ListUnspentAsync(int minConf, int maxConf, List<string> addresses)
        {
            return await _rpcConnector.MakeRequest<List<ListUnspentResponse>>(RpcMethods.listunspent, minConf, maxConf, (addresses ?? new List<string>()));
        }

        public async Task<bool> LockUnspentAsync(bool unlock, IList<ListUnspentResponse> listUnspentResponses)
        {
            IList<object> transactions = new List<object>();

            foreach (var listUnspentResponse in listUnspentResponses)
            {
                transactions.Add(new
                {
                    txid = listUnspentResponse.TxId,
                    listUnspentResponse.Vout
                });
            }

            return await _rpcConnector.MakeRequest<bool>(RpcMethods.lockunspent, unlock, transactions.ToArray());
        }

        public async Task<bool> MoveAsync(string fromAccount, string toAccount, decimal amount, int minConf, string comment)
        {
            return await _rpcConnector.MakeRequest<bool>(RpcMethods.move, fromAccount, toAccount, amount, minConf, comment);
        }

        public async Task PingAsync()
        {
            await _rpcConnector.MakeRequest<string>(RpcMethods.ping);
        }

        public async Task<bool> PrioritiseTransactionAsync(string txId, decimal priorityDelta, decimal feeDelta)
        {
            return await _rpcConnector.MakeRequest<bool>(RpcMethods.prioritisetransaction, txId, priorityDelta, feeDelta);
        }

        public async Task<string> SendFromAsync(string fromAccount, string toBitcoinAddress, decimal amount, int minConf, string comment, string commentTo)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.sendfrom, fromAccount, toBitcoinAddress, amount, minConf, comment, commentTo);
        }

        public async Task<string> SendManyAsync(string fromAccount, Dictionary<string, decimal> toBitcoinAddress, int minConf, string comment)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.sendmany, fromAccount, toBitcoinAddress, minConf, comment);
        }

        public async Task<string> SendRawTransactionAsync(string rawTransactionHexString, bool? allowHighFees)
        {
            return allowHighFees == null
                ? await _rpcConnector.MakeRequest<string>(RpcMethods.sendrawtransaction, rawTransactionHexString)
                : await _rpcConnector.MakeRequest<string>(RpcMethods.sendrawtransaction, rawTransactionHexString, allowHighFees);
        }

        public async Task<string> SendToAddressAsync(string bitcoinAddress, decimal amount, string comment, string commentTo)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.sendtoaddress, bitcoinAddress, amount, comment, commentTo);
        }

        public async Task<string> SetAccountAsync(string bitcoinAddress, string account)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.setaccount, bitcoinAddress, account);
        }

        public async Task<string> SetGenerateAsync(bool generate, short generatingProcessorsLimit)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.setgenerate, generate, generatingProcessorsLimit);
        }

        public async Task<string> SetTxFeeAsync(decimal amount)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.settxfee, amount);
        }

        public async Task<string> SignMessageAsync(string bitcoinAddress, string message)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.signmessage, bitcoinAddress, message);
        }

        public async Task<SignRawTransactionResponse> SignRawTransactionAsync(SignRawTransactionRequest request)
        {
            #region default values

            if (request.Inputs.Count == 0)
            {
                request.Inputs = null;
            }

            if (string.IsNullOrWhiteSpace(request.SigHashType))
            {
                request.SigHashType = SigHashType.All;
            }

            if (request.PrivateKeys.Count == 0)
            {
                request.PrivateKeys = null;
            }

            #endregion

            return await _rpcConnector.MakeRequest<SignRawTransactionResponse>(RpcMethods.signrawtransaction, request.RawTransactionHex, request.Inputs, request.PrivateKeys, request.SigHashType);
        }

        public async Task<string> StopAsync()
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.stop);
        }

        public async Task<string> SubmitBlockAsync(string hexData, params object[] parameters)
        {
            return parameters == null
                ? await _rpcConnector.MakeRequest<string>(RpcMethods.submitblock, hexData)
                : await _rpcConnector.MakeRequest<string>(RpcMethods.submitblock, hexData, parameters);
        }

        public async Task<ValidateAddressResponse> ValidateAddressAsync(string bitcoinAddress)
        {
            return await _rpcConnector.MakeRequest<ValidateAddressResponse>(RpcMethods.validateaddress, bitcoinAddress);
        }

        public async Task<bool> VerifyChainAsync(ushort checkLevel, uint numBlocks)
        {
            return await _rpcConnector.MakeRequest<bool>(RpcMethods.verifychain, checkLevel, numBlocks);
        }

        public async Task<bool> VerifyMessageAsync(string bitcoinAddress, string signature, string message)
        {
            return await _rpcConnector.MakeRequest<bool>(RpcMethods.verifymessage, bitcoinAddress, signature, message);
        }

        public async Task<string> WalletLockAsync()
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.walletlock);
        }

        public async Task<string> WalletPassphraseAsync(string passphrase, int timeoutInSeconds)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.walletpassphrase, passphrase, timeoutInSeconds);
        }

        public async Task<string> WalletPassphraseChangeAsync(string oldPassphrase, string newPassphrase)
        {
            return await _rpcConnector.MakeRequest<string>(RpcMethods.walletpassphrasechange, oldPassphrase, newPassphrase);
        }

        public override string ToString()
        {
            return Parameters.CoinLongName;
        }
    }
}