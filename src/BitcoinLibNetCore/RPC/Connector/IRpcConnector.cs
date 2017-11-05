// Copyright (c) 2014 - 2016 George Kimionis
// See the accompanying file LICENSE for the Software License Aggrement

using BitcoinLib.RPC.Specifications;
using System.Threading.Tasks;

namespace BitcoinLib.RPC.Connector
{
    public interface IRpcConnector
    {
        Task<T> MakeRequest<T>(RpcMethods method, params object[] parameters);
    }
}