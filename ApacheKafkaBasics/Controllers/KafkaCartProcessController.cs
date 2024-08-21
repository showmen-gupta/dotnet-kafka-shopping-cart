// KafkaCartProcessController.cs

using ApacheKafkaBasics.Configuration;
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
    private static CancellationTokenSource? _cancellationTokenSource;

    [HttpPost("StartProducer")]
    public async Task<IActionResult> StartProducer()
    {
        var cartItems = await shoppingCartRepository.GetCartItems();

        if (cartItems.Count <= 0) return BadRequest("no item on the cart to send to kafka producer");

        await kafkaProducerService.CreateKafkaCartTopic(cartItems);

        return Ok("Message sent from producer to consumer to be processed");
    }

    [HttpPost("StartConsumer")]
    public IActionResult StartConsumer()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        KafkaConfiguration.InitiateKafkaConsumer(kafkaConsumerService, _cancellationTokenSource);
        return Ok("Consumer has started to process messages");
    }
    
    [HttpGet("GetAllQueueMessages")]
    public IActionResult GetAllQueueMessages()
    {
        var messages = kafkaConsumerService.GetAllProcessedMessages();
        return Ok(messages);
    }
}