# Solnet.JupiterSwap
 C# Trading Client for Jupiter's v6 Swap API on Solana

## Quickstart
Currently legacy transaction are working & tested.

**Complex versioned transactions need changes and testing to work correctly
```
using Solnet.JupiterSwap;
using Solnet.JupiterSwap.Models;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System.Numerics;

IRpcClient rpcClient = ClientFactory.GetClient("RPC_LINK_HERE");
Account trader = Account.FromSecretKey("PRIVATE_KEY_HERE");
JupiterDexAg jupiterDex = new JupiterDexAg(trader);

PublicKey tokenA = new PublicKey("EsirN3orp85uyvZyDrZnbe9cyo7N1114ynLFdwMPCQce");
PublicKey tokenB = new PublicKey("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v");
PublicKey associatedtrader = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(trader.PublicKey, tokenA);
PublicKey associatedtraderB = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(trader.PublicKey, tokenB);
//amount * 1000000 = USDC lamports
//amount * 1000000000 = SOL lamports
BigInteger amount = new BigInteger(100000);
SwapQuoteAg swapQuote = await jupiterDex.GetSwapQuote(tokenB, tokenA, amount, swapMode: SwapMode.ExactIn, slippageBps: 50);
Transaction _swap_tx = await jupiterDex.Swap(swapQuote, userPublicKey: trader.PublicKey, useSharedAccounts: false, wrapAndUnwrapSol: false);
Message? message = Message.Deserialize(_swap_tx.CompileMessage());
Transaction swap_tx = Transaction.Populate(message);
byte[] _tx = swap_tx.Build(trader);
var response = await rpcClient.SendTransactionAsync(_tx);
Console.WriteLine(response.RawRpcResponse);
Console.ReadKey();
```
