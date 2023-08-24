using Azure.Core;
using Chat_Api.Context;
using Chat_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chat_Api.Controllers
{
    [Route("api/")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("messages")]
        public async Task <IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            // Get the authenticated user's ID from the claims
            var senderId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest("Message content is required");
            }


            var receiver = await _context.Users.FindAsync(request.ReceiverId);

            if (receiver == null)
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
            await _context.SaveChangesAsync();

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


        //edit message
        [HttpPut]
        [Route("messages/{messageId}")]
        public async Task <IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequest request)
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
            await _context.SaveChangesAsync();

            return Ok("Message edited successfully");
        }

        public class EditMessageRequest
        {
            public string Content { get; set; }
        }


        //delete message
        [HttpDelete]
        [Route("messages/{messageId}")]
        public async Task <IActionResult> DeleteMessage(int messageId)
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
            await _context.SaveChangesAsync();

            return Ok("Message deleted successfully");
        }


        // Retrieve Conversation History

        [HttpGet("messages")]
        public IActionResult GetConversationHistory(int userId, DateTime? before = null, int count = 20, string sort = "asc")
        {
            var authenticatedUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var messageQuery = _context.Messages
               .Where(m => (m.SenderId == Convert.ToInt32(authenticatedUserId)) && m.ReceiverId == userId ||
                               (m.SenderId == userId && m.ReceiverId == Convert.ToInt32(authenticatedUserId)))
                .OrderBy(m => sort == "asc" ? m.Timestamp : default(DateTime?))
                .ThenByDescending(m => sort == "desc" ? m.Timestamp : default(DateTime?))
                .Take(count);


            var userIdExists = _context.Users.Any(r => r.Id == userId);

            if (!userIdExists)
            {
                return NotFound("User doesnot exist.");
            }


            var messages = messageQuery.Select(m => new
            {
                id = m.Id,
                senderId = m.SenderId,
                receiverId = m.ReceiverId,
                content = m.Content,
                timestamp = m.Timestamp,
            }).ToList();

            if (messages.Count == 0)
            {
                return NotFound("Conversation is not found");
            }
            if (sort != "asc" && sort != "dsc")
            {
                return BadRequest("Invalid request parametre");
            }

            return Ok(new { messages });
        
        }

    }


}
