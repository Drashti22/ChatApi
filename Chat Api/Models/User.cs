using System.ComponentModel.DataAnnotations;

namespace Chat_Api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }


        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }


        [MinLength(6, ErrorMessage = "Enter 6 letters")]
        public string Password { get; set; }

       public string Token { get; set; }



    }
}
