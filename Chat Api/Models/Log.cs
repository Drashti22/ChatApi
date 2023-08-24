namespace Chat_Api.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string RequestBody { get; set; }
        public DateTime TimeStamp { get; set; }


    }
}
