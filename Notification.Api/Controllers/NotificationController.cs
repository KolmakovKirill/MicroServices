using Microsoft.AspNetCore.Mvc;
using Notification.Shared;
using Notification.Api.Services;

namespace Notification.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly RabbitMqPublisher _publisher;
        private readonly NotificationRepository _repo;
        public NotificationController(RabbitMqPublisher publisher, NotificationRepository repo)
        {
            _publisher = publisher;
            _repo = repo;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] NotificationRequest request)
        {
            await _repo.SaveAsync(request, NotificationStatus.Pending);
            await _publisher.PublishAsync(request);
            return Ok(new { request.Id, Status = "Queued" });
        }

        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var history = await _repo.GetHistoryAsync();
            return Ok(history);
        }
    }
}
