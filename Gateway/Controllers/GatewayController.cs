using CommonShared.Core.Domain;
using CommonShared.Infrastructure.DataStorage.Services;
using CommonShared.Infrastructure.Messaging.Services;
using CommonShared.Core.Domain.DataTransferObjects;
using CommonShared.Infrastructure.Messaging.Models;
using microservices_project.Helpers;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/users")]
public class GatewayController : ControllerBase
{
    private readonly MediaService _mediaService;
    private readonly UserService _userService;
    private readonly KafkaProducerService _producerService;
    private readonly NotificationService _notificationService;
    private readonly NotificationHelper _notificationHelper;

    public GatewayController(MediaService mediaService, UserService userService, KafkaProducerService producerService, NotificationService notificationService, NotificationHelper notificationHelper)
    {
        _mediaService = mediaService;
        _userService = userService;
        _producerService = producerService;
        _notificationService = notificationService;
        _notificationHelper = notificationHelper;
    }


    [HttpGet("")]
    public async Task<ActionResult<UserResponseDTO>> ListUsers()
    {
        var userList = await _userService.ListAsync();
        var dto = userList.Select(u => new UserResponseDTO(u)).ToList();
        return Ok(dto);
    }

    [HttpPost("")]
    public async Task<ActionResult<UserResponseDTO>> CreateUser(CreateUserDTO userDTO)
    {
        try
        {
            var user = new User
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                PhoneNumber = userDTO.PhoneNumber,
                DeviceToken = userDTO.DeviceToken,
                MessengerId = userDTO.MessengerId,
            };
            var createdUser = await _userService.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { userId = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserResponseDTO>> GetUser(long userId)
    {
        var foundUser = await _userService.FindAsync(userId);
        if (foundUser == null) return NotFound();
        var dto = new UserResponseDTO(foundUser);
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


    [HttpPost("{userId}/notificate")]
    public async Task<ActionResult<NotificationResponseDTO>> NotificateUser(long userId, [FromForm] NotificationCreateDTO notificationDTO)
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
        if (notificationDTO.Files != null)
        {
            foreach (var media in notificationDTO.Files)
            {
                var createdMedia = await _mediaService.AddAsync(media, notification);
                medias.Add(createdMedia);
            }
        }

        var topic = _notificationHelper.ResolveTopic(notification.Type);
        await _producerService.SendAsync(topic, notification.Id, notification.Type);
        var dto = new NotificationResponseDTO(notification);
        return Ok(dto);
    }

    [HttpPost("{userId}/notificate_all_channels")]
    public async Task<ActionResult<List<NotificationResponseDTO>>> NotificateUserAllChannel(long userId, [FromForm] AllNotificationCreateDTO notificationCreateDTO)
    {
        
        var foundUser = await _userService.FindAsync(userId);
        if (foundUser == null) return NotFound();

        var result = new List<NotificationResponseDTO>();

        foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
        {  
            var notification = new Notification
            {
                Subject = "я не знаю что это, и что сюда писать",
                Body = notificationCreateDTO.Body,
                Type = type,
                Status = NotificationStatus.Pending,
                UserId = userId,
                User = foundUser
            };
            await _notificationService.AddAsync(notification);

                    var medias = new List<Media>();
            if (notificationCreateDTO.Files != null)
            {
                foreach (var media in notificationCreateDTO.Files)
                {
                    var createdMedia = await _mediaService.AddAsync(media, notification);
                    medias.Add(createdMedia);
                }
            }
            

            var topic = _notificationHelper.ResolveTopic(notification.Type);
            await _producerService.SendAsync(topic, notification.Id, notification.Type);

            result.Add(new NotificationResponseDTO(notification));
        }

        return Ok(result);
    }
}

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    
    [HttpGet("status/{notificationId}")]    
    public async Task<ActionResult<NotificationResponseDTO>> GetNotification(long notificationId)
    {
        var foundNotification = await _notificationService.FindAsync(notificationId);
        if (foundNotification == null) return NotFound();
        var dto = new NotificationResponseDTO(foundNotification);
        return Ok(dto);
    }
}  