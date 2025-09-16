<div align="center">

# Solnet.JupiterSwap
<strong>C# Client Wrapper for Jupiter Aggregator v6 Swap API (Solana)</strong>

[Jupiter](https://jup.ag) | [Solnet](https://github.com/bmresearch/Solnet) | .NET 8

</div>

## Overview
`Solnet.JupiterSwap` is a lightweight, strongly-typed C# wrapper around Jupiter's v6 Swap Aggregator API. It streamlines:

- Fetching optimal swap routes (quotes)
- Building unsigned swap transactions (legacy by default)
- Token metadata retrieval (strict vs full list)
- Simple interop with existing `Solnet` wallet, RPC and program utilities

> Status: Legacy (non-versioned) transaction flow is implemented and tested. Versioned (v0) transactions require additional work (see Roadmap).

## Features
- Quote retrieval with configurable slippage & routing constraints
- Swap transaction generation (unsigned) with support for:
	- Shared accounts toggle
	- Auto wrap/unwrap SOL
	- Legacy transaction selection
- Token list hydration (strict / all)
- High-level strongly typed models: `SwapQuoteAg`, `SwapRequest`, `SwapResponse`, `TokenData`
- Simple extension entry point via `JupiterDexAg : IDexAggregator`

## Installation
Package not yet published to NuGet (if you need this, open an issue). For now:

1. Clone the repository
2. Add the `Solnet.JupiterSwap` project to your solution
3. Reference it from your application project

Prerequisites:
- .NET 8 SDK
- A Solana RPC endpoint (HTTPS) with sufficient rate limits
- A funded keypair for signing & paying network fees

## Quick Start
Below is a minimal end‑to‑end example performing a USDC -> SOL style swap (adjust mints + amount as needed).

```csharp
using Solnet.JupiterSwap;
using Solnet.JupiterSwap.Models;
using Solnet.JupiterSwap.Types;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Programs;
using Solnet.Wallet;
using System.Numerics;

// 1. RPC + wallet
IRpcClient rpc = ClientFactory.GetClient("https://your-rpc-endpoint");
Account trader = Account.FromSecretKey("<BASE58_SECRET_KEY>");

// 2. Aggregator client (optionally pass PublicKey)
var jupiter = new JupiterDexAg(trader.PublicKey);

// 3. Define mints (USDC -> SOL example; replace if needed)
PublicKey inputMint = new("EPjFWdd5AufqSSqeM2qN1xzybapC8G4wEGGkZwyTDt1v"); // USDC
PublicKey outputMint = new("So11111111111111111111111111111111111111112");   // SOL (wrapped)

// 4. Amounts are in base units (lamports)
// USDC has 6 decimals -> 1.5 USDC = 1_500_000
BigInteger amountIn = new BigInteger(1_500_000);

// 5. Get quote (ExactIn with 0.50% slippage)
SwapQuoteAg quote = await jupiter.GetSwapQuote(
		inputMint,
		outputMint,
		amountIn,
		swapMode: SwapMode.ExactIn,
		slippageBps: 50 // 50 bps = 0.50%
);

// 6. Build unsigned legacy transaction
Transaction unsignedSwap = await jupiter.Swap(
		quote,
		userPublicKey: trader.PublicKey,
		useSharedAccounts: false,
		wrapAndUnwrapSol: true,
		asLegacy: true
);

// 7. (OPTIONAL) Rebuild message & sign (pattern defensive vs direct Build)
Message? compiled = Message.Deserialize(unsignedSwap.CompileMessage());
Transaction finalTx = Transaction.Populate(compiled);
byte[] signed = finalTx.Build(trader);

// 8. Send
var sendResult = await rpc.SendTransactionAsync(signed);
Console.WriteLine(sendResult.RawRpcResponse);
```

### Amount Helper Reference
- SOL: 1 SOL = 1_000_000_000 lamports (9 decimals)
- USDC: 1 USDC = 1_000_000 (6 decimals)

## API Surface

### Entry Point: `JupiterDexAg`
Constructor overloads:
- `JupiterDexAg(string endpoint = "https://quote-api.jup.ag/v6")`
- `JupiterDexAg(PublicKey account, string endpoint = ...)`

Primary methods (via `IDexAggregator`):
| Method | Purpose |
| ------ | ------- |
| `GetSwapQuote(...)` | Retrieve best route & amounts |
| `Swap(...)` | Construct unsigned swap transaction |
| `GetTokens(tokenListType)` | Fetch token metadata list |
| `GetTokenBySymbol(symbol)` | Lookup token by symbol |
| `GetTokenByMint(mint)` | Lookup token by mint |

### GetSwapQuote Parameters
- `inputMint` / `outputMint` (PublicKey)
- `amount` (`BigInteger`) – Interpretation depends on `swapMode`:
	- `ExactIn`: amount is input token quantity
	- `ExactOut`: amount is desired output token quantity
- `swapMode`: `ExactIn | ExactOut`
- `slippageBps`: optional (basis points)
- `excludeDexes`: e.g. `["Saber","Aldrin"]`
- `onlyDirectRoutes`: restrict to single hop
- `platformFeeBps`: optional fee charged (output token for ExactIn, input for ExactOut)
- `maxAccounts`: rough upper bound for account planning

### Swap Parameters (current implementation subset)
- `quoteResponse`: required `SwapQuoteAg` from previous step
- `userPublicKey`: wallet authority (falls back to constructor account)
- `destinationTokenAccount`: optional explicit output ATA
- `wrapAndUnwrapSol`: auto create/close temporary WSOL ATA
- `useSharedAccounts`: use aggregator shared program accounts
- `asLegacy`: request legacy transaction (default true in this wrapper)

> Not yet exposed here: dynamic compute unit tuning, token ledger, referral fee account injection (see `SwapRequest` model fields). They can be added with minor extension work (see Contributing).

## Data Models

### SwapQuoteAg
Key fields:
- `InAmount`, `OutAmount` (string raw) + parsed `InputAmount`, `OutputAmount`
- `InputMint`, `OutputMint`
- `SlippageBps`
- `RoutePlan` (list of route legs with AMM metadata)
- `PriceImpactPct`, `ContextSlot`, `TimeTaken`

### RoutePlan / SwapInfo
Per-hop AMM route data: amounts, fee mint, label + AMM key.

### SwapRequest (internal when calling `/swap`)
Contains flags for advanced behaviors (ledger, referral, compute unit strategies, blockhash expiry tuning).

### SwapResponse
Returns base64 serialized transaction (`SwapTransaction`) ready for deserialization & signing.

### TokenData
Includes `Name`, `Mint`, `Symbol`, `Decimals`, `LogoURI`, optional Coingecko ID, flags: `Whitelisted`, `PoolToken`.

## Token Lists
`GetTokens(TokenListType.Strict)` fetches the curated list.
`TokenListType.All` returns a superset including more experimental or newly added assets.

Caching: First call caches token list in-memory for the lifetime of the `JupiterDexAg` instance.

## Error Handling
Network / API failures throw `HttpRequestException` with status code context.
Potential sources:
- Invalid mint addresses
- Route not found (upstream returns non-success)
- RPC later rejects swap (slippage exceeded, blockhash expired, account constraints)

Recommended patterns:
```csharp
try
{
		var quote = await jupiter.GetSwapQuote(inputMint, outputMint, amountIn, slippageBps: 30);
}
catch (HttpRequestException ex)
{
		// log + classify (ex.StatusCode isn't directly on exception pre .NET 5, inspect Message)
}
```

## Extending / Versioned Transactions
Jupiter v6 can return versioned (v0) transactions. Current wrapper forces legacy by sending `asLegacyTransaction=true` both on quote and swap. To add versioned support:
1. Expose a public `asLegacy` boolean on `GetSwapQuote` (currently hardcoded to true)
2. When `false`, pass `asLegacyTransaction=false` to both endpoints
3. Deserialize returned transaction and sign using Solnet's versioned transaction pathway
4. Adjust docs + tests

## Roadmap
- [ ] Versioned transaction support
- [ ] Referral fee account wiring (`feeAccount` + `platformFeeBps` synergy)
- [ ] Dynamic compute unit limit & pricing controls
- [ ] Optional token ledger workflow
- [ ] Test suite (integration harness against a local validator or devnet)
- [ ] NuGet package publishing & semantic versioning

## Contributing
PRs welcome. Please:
1. Open an issue describing the change
2. Keep changes focused (one feature / fix per PR)
3. Include summary in the PR description

Suggested future improvements: typed errors, resilience policies (retry/backoff), structured logging hooks, metrics instrumentation, cancellation token support.

## Security Notes
- Always validate mints & amounts before relaying user intent
- Consider quoting and immediately executing to reduce route drift
- Monitor slippage tolerance values — 500 bps (5%) is usually too high for stable pairs

## License
Distributed under the MIT License. See `LICENSE` for details.

## Disclaimer
Use at your own risk. On-chain interactions involve financial risk; verify outputs and test on devnet when possible.

## Support
Open an issue for bugs, feature requests, or questions. If this project helps you ship, consider a star.

---
Happy building on Solana with Jupiter + C# 🚀
