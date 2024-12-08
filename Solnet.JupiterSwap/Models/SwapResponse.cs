using Newtonsoft.Json;

namespace Solnet.JupiterSwap.Models
{
    /// <summary>
    /// The response to get a swap quote from the Jupiter aggregator.
    /// </summary>
    public class SwapResponse
    {
        /// <summary>
        /// The swap quote
        /// </summary>
        public string? SwapTransaction { get; set; }

        [JsonProperty("lastValidBlockHeight")]
        private string? _lastValidBlockHeight { get; set; }

    }
}