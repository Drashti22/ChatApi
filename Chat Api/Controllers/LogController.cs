using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Chat_Api.Context;
using Chat_Api.Models;


namespace Chat_Api.Controllers
{
    [Authorize]
    [Route("api/")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LogController(AppDbContext context)
        {
            _context = context;
        }

        //[HttpGet]
        //[Authorize]

        //public async Task<IActionResult> GetLogs([FromQuery] DateTime? startTime = null, [FromQuery] DateTime? endTime = null)
        //{
        //    if (startTime == null)
        //        startTime = DateTime.UtcNow.AddMinutes(-5);

        //    if (endTime == null)
        //        endTime = DateTime.UtcNow;

        //    var logs = await _context.logs.
        //        Where(log => log.TimeStamp >= startTime && log.TimeStamp <= endTime).ToListAsync();

        //    if (logs.Count == 0)
        //        return NotFound();
             
        //    return Ok(logs);


        //}
        [HttpGet("log")]
        public async Task<IActionResult> GetLogs([FromQuery] string startTime = null, [FromQuery] string endTime = null)
        {
            DateTime? parsedStartTime = ParseDateTime(startTime);
            DateTime? parsedEndTime = ParseDateTime(endTime);

            if (parsedStartTime == null)
                parsedStartTime = DateTime.Now.AddMinutes(-5);

            if (parsedEndTime == null)
                parsedEndTime = DateTime.Now;

            var logs = await _context.logs
                .Where(log => log.TimeStamp >= parsedStartTime && log.TimeStamp <= parsedEndTime)
                .ToListAsync();

            if (logs.Count == 0)
                return NotFound();

            return Ok(logs);
        }

        private DateTime? ParseDateTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString))
                return null;

            if (DateTime.TryParse(dateTimeString, out DateTime parsedDateTime))
                return parsedDateTime;

            return null;
        }
    }


}

