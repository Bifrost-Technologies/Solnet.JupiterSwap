namespace Solnet.JupiterSwap.Models
{
    public class SwapRequest
    {

        /// <summary>
        /// The swap quote
        /// </summary>
        public required SwapQuoteAg quoteResponse { get; set; }

        /// <summary>
        /// The user public key.
        /// </summary>
        public required string userPublicKey { get; set; }

        /// <summary>
        /// Default is true. If true, will automatically wrap/unwrap SOL. If false, it will use wSOL token account. 
        /// Will be ignored if `destinationTokenAccount` is set because the `destinationTokenAccount` may belong to a different user that we have no authority to close.
        /// </summary>
        public bool? wrapAndUnwrapSol { get; set; } = true;

        /// <summary>
        /// Default is true. This enables the usage of shared program accounts. 
        /// That means no intermediate token accounts or open orders accounts need to be created for the users. 
        /// But it also means that the likelihood of hot accounts is higher.
        /// </summary>
        public bool? useSharedAccounts { get; set; } = true;

        /// <summary>
        /// Fee token account, same as the output token for ExactIn and as the input token for ExactOut, 
        /// it is derived using the seeds = ["referral_ata", referral_account, mint] and the `REFER4ZgmyYx9c6He5XfaTMiGfdLwRnkV4RPp9t9iF3` referral contract 
        /// (only pass in if you set a feeBps and make sure that the feeAccount has been created).
        /// </summary>
        public string? feeAccount { get; set; }


        /// <summary>
        /// Default is false. Request a legacy transaction rather than the default versioned transaction, 
        /// needs to be paired with a quote using asLegacyTransaction otherwise the transaction might be too large.
        /// </summary>
        public bool? asLegacyTransaction { get; set; } = true;

        /// <summary>
        /// Default is false. This is useful when the instruction before the swap has a transfer that increases the input token amount. 
        /// Then, the swap will just use the difference between the token ledger token amount and post token amount.
        /// </summary>
        public bool? useTokenLedger { get; set; } = false;

        /// <summary>
        /// Public key of the token account that will be used to receive the token out of the swap. 
        /// If not provided, the user's ATA will be used. If provided, we assume that the token account is already initialized.
        /// </summary>
        public string? destinationTokenAccount { get; set; }

        /// <summary>
        /// When enabled, it will do a swap simulation to get the compute unit used and set it in ComputeBudget's compute unit limit. 
        /// This will increase latency slightly since there will be one extra RPC call to simulate this. Default is `false`.
        /// </summary>
        public bool? DynamicComputeUnitLimit { get; set; } = false;

        /// <summary>
        /// When enabled, it will not do any rpc calls check on user's accounts. 
        /// Enable it only when you already setup all the accounts needed for the transaction, like wrapping or unwrapping sol, destination account is already created.
        /// </summary>
        public bool? SkipUserAccountsRpcCalls { get; set; } = false;

        /// <summary>
        /// The program authority id [0;7], load balanced across the available set by default.
        /// </summary>
        public int? ProgramAuthorityId { get; set; }

        /// <summary>
        /// Default is false. Enabling it would reduce use an optimized way to open WSOL that reduce compute unit.
        /// </summary>
        public bool? AllowOptimizedWrappedSolTokenAccount { get; set; } = false;


        /// <summary>
        /// Optional. When passed in, Swap object will be returned with your desired slots to expiry.
        /// </summary>
        public int? BlockhashSlotsToExpiry { get; set; }

        /// <summary>
        /// Optional. Default to false. Request Swap object to be returned with the correct blockhash prior to Agave 2.0.
        /// </summary>
        public bool? CorrectLastValidBlockHeight { get; set; } = false;
    }

}