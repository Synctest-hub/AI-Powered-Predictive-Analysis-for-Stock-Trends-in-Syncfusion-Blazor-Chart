namespace AIStockTrends
{
    internal class ChatCompletionsOptions
    {
        public string DeploymentName { get; set; }
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
        public float NucleusSamplingFactor { get; set; }
        public int FrequencyPenalty { get; set; }
        public int PresencePenalty { get; set; }
    }
}