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
        await Task.Run(() => kafkaConsumerService.StartCartConsumer(cancellationTokenSource.Token), cancellationTokenSource.Token);
        return Ok("Consumer has started to process messages");
    }

    [HttpGet("GetAllQueueMessages")]
    public IActionResult GetAllQueueMessages()
    {
        var messages = kafkaConsumerService.GetAllMessages();
        return Ok(messages);
    }

    [HttpPost("accept")]
    public IActionResult Accept()
    {
        //if (kafkaConsumerService.TryDequeueMessage(out var message))
        // Logic for accepting the message
        // E.g., save to database or mark as processed
        //return Ok($"Accepted message: {message}");

        return NotFound("No messages to accept");
    }

    [HttpPost("reject")]
    public IActionResult Reject()
    {
        //if (kafkaConsumerService.TryDequeueMessage(out var message))
        // Logic for rejecting the message
        // E.g., log the rejection or send to a dead-letter queue
        //return Ok($"Rejected message: {message}");

        return NotFound("No messages to reject");
    }
}