// KafkaCartProcessController.cs

using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApacheKafkaBasics.Controllers;

[ApiController]
[Route("[controller]")]
public class KafkaCartProcessController(
    KafkaProducerService kafkaProducerService,
    KafkaConsumerService kafkaConsumerService,
    IShoppingCartRepository shoppingCartRepository)
    : Controller
{
    [HttpPost("StartProducer")]
    public async Task<IActionResult> StartProducer()
    {
        var cartItems = await shoppingCartRepository.GetCartItems();

        if (cartItems.Count <= 0) return BadRequest("no item on the cart to send to kafka producer");

        await kafkaProducerService.CreateKafkaCartTopic(cartItems);

        return Ok("Message sent from producer to consumer to be processed");
    }

    [HttpPost("StartConsumer")]
    public async Task<IActionResult> StartConsumer()
    {
        // Start Kafka consumer in a background task
        var cancellationTokenSource = new CancellationTokenSource();
        await Task.Run(() => kafkaConsumerService.StartCartConsumer(cancellationTokenSource.Token),
            cancellationTokenSource.Token);
        return Ok("Consumer has started to process messages");
    }

    [HttpPost("StartCartItemProcessor")]
    public async Task<IActionResult> StartCartItemProcessor(int productId, bool isApproved)
    {
        // Start Kafka consumer in a background task
        var cancellationTokenSource = new CancellationTokenSource();
        await kafkaConsumerService.StartCartItemProcessor(productId, isApproved, cancellationTokenSource);
        return Ok("Consumer has started to process messages");
    }

    [HttpGet("GetAllQueueMessages")]
    public IActionResult GetAllQueueMessages()
    {
        var messages = kafkaConsumerService.GetAllProcessedMessages();
        return Ok(messages);
    }
}