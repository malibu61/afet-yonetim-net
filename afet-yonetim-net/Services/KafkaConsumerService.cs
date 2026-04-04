using afet_yonetim_net.Models;
using afet_yonetim_net.Services;
using Confluent.Kafka;
using System.Text.Json;

public class KafkaConsumerService : BackgroundService
{
    private readonly string _topic = "dbserver1.public.depremler";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "afet-api-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // EKSİK OLAN SATIR BURASI: Kafka'dan bir mesaj bekle ve çek
                var result = consumer.Consume(stoppingToken);

                if (result != null && result.Message != null)
                {
                    var jsonDoc = JsonDocument.Parse(result.Message.Value);
                    var afterElement = jsonDoc.RootElement.GetProperty("payload").GetProperty("after");

                    var yeniDeprem = new Deprem
                    {
                        id = afterElement.GetProperty("id").GetInt32(),
                        mag = afterElement.GetProperty("mag").GetDouble(),
                        yer = afterElement.GetProperty("yer").GetString(),
                        enlem = afterElement.GetProperty("enlem").GetDouble(),
                        boylam = afterElement.GetProperty("boylam").GetDouble(),
                        zaman = ParseDebeziumTime(afterElement.GetProperty("zaman"))
                    };

                    // Bellekteki listeye ekle
                    DepremStore.SonDepremler.Insert(0, yeniDeprem);

                    if (DepremStore.SonDepremler.Count > 50)
                        DepremStore.SonDepremler.RemoveAt(50);

                    Console.WriteLine($"Kafka'dan yakalandı: {yeniDeprem.yer}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
            }
        }
    }

    private DateTime ParseDebeziumTime(JsonElement timeElement)
    {
        // Debezium zamanı bazen string bazen long (timestamp) gönderir
        if (timeElement.ValueKind == JsonValueKind.Number)
        {
            // Eğer sayı geliyorsa (genelde milisaniye/mikrosaniye bazlıdır)
            return DateTimeOffset.FromUnixTimeMilliseconds(timeElement.GetInt64() / 1000).DateTime;
        }

        // Eğer string geliyorsa
        if (DateTime.TryParse(timeElement.GetString(), out DateTime dt))
        {
            return dt;
        }

        return DateTime.Now; // Hiçbiri olmazsa fallback
    }
}

