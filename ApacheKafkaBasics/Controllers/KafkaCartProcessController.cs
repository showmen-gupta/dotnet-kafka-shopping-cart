// KafkaController.cs

using ApacheKafkaBasics.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApacheKafkaBasics.Controllers;

[ApiController]
[Route("[controller]")]
public class KafkaCartProcessController(KafkaProducerService kafkaProducerService, KafkaConsumerService kafkaConsumerService)
    : Controller
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string message)
    {

        await kafkaProducerService.ProduceAsync("cart-topic", message);
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