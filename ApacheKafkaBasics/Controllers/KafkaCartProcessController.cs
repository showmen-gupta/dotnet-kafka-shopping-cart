// KafkaCartProcessController.cs

using ApacheKafkaBasics.Configuration;
using ApacheKafkaBasics.Interfaces;
using ApacheKafkaBasics.Services;
using Generated.Entity;
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

        var cartItemsAvro = (from item in cartItems
                let productsToAdd =
                    new Product
                    {
                        ProductId = item.ProductDto.ProductId, Name = item.ProductDto.Name,
                        Price = item.ProductDto.Price
                    }
                select new CartItem { Product = productsToAdd, Quantity = item.Quantity, TotalPrice = item.TotalPrice })
            .ToList();

        if (cartItems.Count <= 0) return BadRequest("no item on the cart to send to kafka producer");

        await kafkaProducerService.CreateKafkaCartTopic(cartItemsAvro);

        return Ok("Message sent from producer to consumer to be processed");
    }

    [HttpPost("StartConsumer")]
    public IActionResult StartConsumer()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        KafkaConfiguration.InitiateKafkaConsumer(kafkaConsumerService, _cancellationTokenSource);
        return Ok("Consumer has started to process messages");
    }

    [HttpGet("GetAllQueuedMessages")]
    public IActionResult GetAllQueuedMessages()
    {
        var messages = kafkaProducerService.GetAllQueuedMessages();
        return Ok(messages);
    }

    [HttpGet("GetAllProcessedMessages")]
    public IActionResult GetAllProcessedMessages()
    {
        var messages = kafkaConsumerService.GetAllProcessedMessages();
        return Ok(messages);
    }
}