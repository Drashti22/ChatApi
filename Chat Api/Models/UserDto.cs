using System.ComponentModel.DataAnnotations;

namespace Chat_Api.Models
{
    public class UserDto
    {

        [MinLength(6, ErrorMessage = "Enter 6 letters")]
        public string Email { get; set; }


        [MinLength(6, ErrorMessage = "Enter 6 letters")]
        public string Password { get; set; }
    }
}
