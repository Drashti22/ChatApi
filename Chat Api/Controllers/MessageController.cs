using Chat_Api.Context;
using Chat_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chat_Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] SendMessageRequest request)
        {
            // Get the authenticated user's ID from the claims

            if(request == null  || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Message content is required");
            }

            var senderId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var receiverExists = _context.Users.Any(r => r.Id != request.ReceiverId);

            if (receiverExists)
            {
                return NotFound("Receiver not found");
            }

            //create and save the message
            var message = new Message
            {
                SenderId =Convert.ToInt32(senderId),
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                Timestamp = DateTime.UtcNow

            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            // Return the created message details in the response
            var response = new SendMessageResponse
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                Timestamp = message.Timestamp
            };
            return Ok(response);

        }


        //edit message
        [HttpPut]
        [Route("api/messages/{messageId}")]
        public IActionResult EditMessage(int messageId, [FromBody] EditMessageRequest request)
        {
            var authenticatedUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Updated message content is required");
            }

            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId);

            if (message == null)
            {
                return NotFound("Message not found");
            }

            if (message.SenderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }

            message.Content = request.Content;
            _context.SaveChanges();

            return Ok("Message edited successfully");
        }

        public class EditMessageRequest
        {
            public string Content { get; set; }
        }
        //delete message
        [HttpDelete]
        [Route("api/messages/{messageId}")]
        public IActionResult DeleteMessage(int messageId)
        {
            var authenticatedUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = _context.Messages.FirstOrDefault(m => m.Id == messageId);

            if (message == null)
            {
                return NotFound("Message not found");
            }

            if (message.SenderId != Convert.ToInt32(authenticatedUserId))
            {
                return Unauthorized();
            }

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return Ok("Message deleted successfully");
        }

    }

    public class SendMessageRequest
        {
            public int ReceiverId { get; set; }
            public string Content { get; set; }
        }

        public class SendMessageResponse
        {
            public int MessageId { get; set; }
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
