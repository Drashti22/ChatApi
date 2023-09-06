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


        [HttpGet("log")]
        public async Task<IActionResult> GetLogs([FromQuery]string timeframe, [FromQuery] string startTime = null, [FromQuery] string endTime = null)
        {
            DateTime? parsedStartTime = ParseDateTime(startTime);
            DateTime? parsedEndTime = ParseDateTime(endTime);

            if (parsedStartTime == null)
                parsedStartTime = DateTime.Now.AddMinutes(-5);

            if (parsedEndTime == null)
                parsedEndTime = DateTime.Now;

            switch (timeframe)
            {
                //case "Last 5 mins":
                //    parsedStartTime = DateTime.Now.AddMinutes(-5);
                //    break;
                //case "Last 10 mins":
                //    parsedStartTime = DateTime.Now.AddMinutes(-10);
                //    break;
                //case "Last 30 mins":
                //    parsedStartTime = DateTime.Now.AddMinutes(-30);
                //    break;
                //case "custom":
                //    parsedStartTime = parsedStartTime ?? DateTime.Now.AddMinutes(-30);
                //    parsedEndTime = parsedEndTime ?? DateTime.Now;

                //    break;
                default:
                    break;
            }
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

