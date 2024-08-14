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
    IShoppingCart shoppingCart)
    : Controller
{
    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var cartItems = await shoppingCart.GetCartItems();
        
        if (cartItems.Count <= 0) return BadRequest("no item on the cart to send to kafka producer");
        
        const string topicName = "in-cart-items";

        await kafkaProducerService.CreateKafkaCartTopic(cartItems);
        return Ok("Message sent to Kafka");

    }

    [HttpGet]
    public IActionResult Get()
    {
        var messages = kafkaConsumerService.GetAllMessages();
        return Ok(messages);
    }

    [HttpPost("accept")]
    public IActionResult Accept()
    {
        if (kafkaConsumerService.TryDequeueMessage(out var message))
            // Logic for accepting the message
            // E.g., save to database or mark as processed
            return Ok($"Accepted message: {message}");

        return NotFound("No messages to accept");
    }

    [HttpPost("reject")]
    public IActionResult Reject()
    {
        if (kafkaConsumerService.TryDequeueMessage(out var message))
            // Logic for rejecting the message
            // E.g., log the rejection or send to a dead-letter queue
            return Ok($"Rejected message: {message}");

        return NotFound("No messages to reject");
    }
}