// using microservices_project.Core.Domain;
// using microservices_project.Infrastructure.DataStorage.Services;
// using microservices_project.Infrastructure.Messaging.Services;
// using Microsoft.AspNetCore.Mvc;

// [ApiController]
// [Route("api/media")]
// public class MediaController : ControllerBase
// {
//     private readonly MediaService _service;
//     public MediaController(MediaService service)
//     {
//         _service = service;
//     }


//     [HttpPost("upload")]
//     public async Task<ActionResult<Media>> UploadMedia(IFormFile file)
//     {
//         try
//         {
//             var media = await _service.AddAsync(file);
//             return CreatedAtAction(nameof(GetMedia), new { id = media.Id }, media);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine(ex);
//             return BadRequest(ex.Message);
//         }
//     }

//     [HttpGet("{id}")]
//     public async Task<ActionResult<Media>> GetMedia(int id)
//     {
//         var media = await _service.FindAsync(id);
//         if (media == null) return NotFound();
//         return Ok(media);
//     }

//     [HttpGet("{id}/url")]
//     public async Task<ActionResult<Media>> GetMediaUrl(int id)
//     {
//         var media = await _service.FindAsync(id);
//         if (media == null) return NotFound();
//         var url = await _service.GetPresignedUrlAsync(media.Source);
//         return Ok(new {url});
//     }

//     [HttpDelete("{id}")]
//     public async Task<IActionResult> DeleteMedia(int id)
//     {
//         var success = await _service.RemoveAsync(id);
//         if (!success) return NotFound();
//         return NoContent();
//     }
// }

// [ApiController]
// [Route("api/[controller]")]
// public class KafkaController : ControllerBase
// {
//     private readonly KafkaProducerService _kafka;

//     public KafkaController(KafkaProducerService kafka)
//     {
//         _kafka = kafka;
//     }

//     [HttpPost("send")]
//     public async Task<IActionResult> Send(
//         [FromQuery] string topic,
//         [FromBody] string message,
//         CancellationToken ct)
//     {
//         await _kafka.SendAsync(topic, message, ct);
//         return Ok();
//     }
// }