namespace ApacheKafkaBasics.Configuration;

public class KafkaConfigSecrets
{
    public string SchemaRegistry { get; set; } = default!;
    public string KafkaBroker { get; set; } = default!;
    public string KafkaGroupId { get; set; } = default!;
    public string KafkaTopic { get; set; } = default!;
    public string KafkaProcessedTopic { get; set; } = default!;
}