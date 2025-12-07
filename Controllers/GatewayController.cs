using microservices_project.Core.Domain;
using microservices_project.Infrastructure.DataStorage.Services;
using microservices_project.Infrastructure.Messaging.Services;
using microservices_project.Core.Domain.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class GatewayController : ControllerBase
{
    private readonly MediaService _mediaService;
    private readonly UserService _userService;
    private readonly KafkaProducerService _producerService;
    private readonly NotificationService _notificationService;

    public GatewayController(MediaService mediaService, UserService userService, KafkaProducerService producerService, NotificationService notificationService)
    {
        _mediaService = mediaService;
        _userService = userService;
        _producerService = producerService;
        _notificationService = notificationService;
    }

    [HttpGet("")]
    public async Task<ActionResult<User>> ListUsers()
    {
        var userList = _userService.ListAsync();
        return Ok(userList);
    }

    [HttpPost("")]
    public async Task<ActionResult<User>> CreateUser(CreateUserDTO userDTO)
    {
        try
        {
            var user = new User
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                PhoneNumber = userDTO.PhoneNumber,
            };
            var createdUser = await _userService.AddAsync(user);
            Console.WriteLine($"Created User ID: {createdUser.Id}"); 
            return CreatedAtAction(nameof(GetUser), new { userId = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<User>> GetUser(long userId)
    {
        var foundUser = await _userService.FindAsync(userId);
        if (foundUser == null) return NotFound();
        return Ok(foundUser);
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<User>> RemoveUser(long userId)
    {
        var foundUser = await _userService.FindAsync(userId);
        if (foundUser == null) return NotFound();
        await _userService.RemoveAsync(foundUser);
        return NoContent();
    }

    // [HttpGet("{notificationId}")]    
    // public async Task<ActionResult<Notification>> GetNotification(long notificationId)
    // {
    //     var foundNotification = await _notificationService.FindAsync(notificationId);
    //     if (foundNotification == null) return NotFound();
    //     return Ok(foundNotification);
        
    // }

    [HttpPost("{userId}/notificate")]
    public async Task<ActionResult<bool>> NotificateUser(long userId, [FromForm] NotificationCreateDTO notificationDTO)
    {
        var foundUser = await _userService.FindAsync(userId);
        if (foundUser == null) return NotFound();

        var notification = new Notification
        {
            Subject = "я не знаю что это, и что сюда писать",
            Body = notificationDTO.Body,
            Type = notificationDTO.Type,
            Status = NotificationStatus.Pending,
            UserId = userId,
            User = foundUser
        };
        await _notificationService.AddAsync(notification);

        var medias = new List<Media>();
        foreach (var media in notificationDTO.Files)
        {
            var createdMedia = await _mediaService.AddAsync(media, notification);   // ДА, КОСТЫЛЬ, НО У МЕНЯ НЕТ ВРЕМЕНИ ПЕРЕДЕЛАТЬ =(
            medias.Add(createdMedia);
        }


        await _producerService.SendAsync("notifications", notification.Id.ToString()); //Пока что пусть будет так, потом переделаю в Json (я подустал)
        return Ok(new
        {
            Notification = new NotificationResponseDTO(notification)
        });

    }
}

