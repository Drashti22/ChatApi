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
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
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
                SenderId = Convert.ToInt32(senderId),
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
        public async Task<IActionResult> EditMessage(int messageId, [FromBody] EditMessageRequest request)
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

            return Ok(new { message = "Message edited successfully" });
        }

        public class EditMessageRequest
        {
            public string Content { get; set; }
        }


        //delete message
        [HttpDelete]
        [Route("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
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

            return Ok(new { message = "Message delete successfully" });
        }


        // Retrieve Conversation History

        //    [HttpGet("messages")]
        //    public IActionResult GetConversationHistory(int userId, string? before = null, int count = 20, string sort = "asc")
        //    {
        //        var authenticatedUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //        // Get the total message count for the user


        //        // Calculate the skip count based on the total number of messages and count requested


        //        var messageQuery = _context.Messages
        //           .Where(m => (m.SenderId == Convert.ToInt32(authenticatedUserId)) && m.ReceiverId == userId ||
        //                           (m.SenderId == userId && m.ReceiverId == Convert.ToInt32(authenticatedUserId)))
        //            .OrderBy(m => sort == "asc" ? m.Timestamp : default(DateTime?))
        //            .ThenByDescending(m => sort == "desc" ? m.Timestamp : default(DateTime?))
        //            .Take(count);


        //        var userIdExists = _context.Users.Any(r => r.Id == userId);

        //        if (!userIdExists)
        //        {
        //            return NotFound("User does not exist.");
        //        }

        //        var messages = messageQuery.Select(m => new
        //        {
        //            id = m.Id,
        //            senderId = m.SenderId,
        //            receiverId = m.ReceiverId,
        //            content = m.Content,
        //            timestamp = m.Timestamp,
        //        }).ToList();

        //        if (messages.Count == 0)
        //        {
        //            return Ok (new List<object>());
        //        }
        //        if (sort != "asc" && sort != "desc")
        //        {
        //            return BadRequest("Invalid request parameter");
        //        }

        //        // Reverse the messages if the sort order is ascending
        //        if (sort == "asc")
        //        {
        //            messages.Reverse();
        //        }


        //        return Ok(new { messages });
        //    }


        //}
        [HttpGet("messages")]
        public IActionResult GetConversationHistory(int userId, string? before = null, int count = 20, string sort = "asc")
        {
            var authenticatedUserId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Initialize the message query
            var messageQuery = _context.Messages
                .Where(m => (m.SenderId == Convert.ToInt32(authenticatedUserId)) && m.ReceiverId == userId ||
                             (m.SenderId == userId && m.ReceiverId == Convert.ToInt32(authenticatedUserId)));

            // Filter messages based on the 'before' timestamp if provided
            if (!string.IsNullOrEmpty(before) && DateTime.TryParse(before, out var beforeTime))
            {
                if (sort == "asc")
                {
                    messageQuery = messageQuery
                        .Where(m => m.Timestamp < beforeTime)
                        .OrderBy(m => m.Timestamp);
                }
                else if (sort == "desc")
                {
                    messageQuery = messageQuery
                        .Where(m => m.Timestamp < beforeTime)
                        .OrderByDescending(m => m.Timestamp);
                }
            }
            else
            {
                // No 'before' timestamp provided, apply sorting based on the 'sort' parameter
                messageQuery = sort == "asc"
                    ? messageQuery.OrderByDescending(m => m.Timestamp)
                    : messageQuery.OrderBy(m => m.Timestamp);
            }

            // Take the specified number of messages
            var messages = messageQuery
                .Take(count)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    receiverId = m.ReceiverId,
                    content = m.Content,
                    timestamp = m.Timestamp,
                }).ToList();

            var userIdExists = _context.Users.Any(r => r.Id == userId);

            if (!userIdExists)
            {
                return NotFound("User does not exist.");
            }

            if (messages.Count == 0)
            {
                return Ok(new List<object>());
            }

            return Ok(new { messages });
        }

    }
}
