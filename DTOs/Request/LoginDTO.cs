using System.ComponentModel.DataAnnotations;

namespace TagerCom.ViewModels
{
    public class LoginDTO
    {
        [Required]
        public string EmailOrUserName { get; set; } =string.Empty;

        [Required,DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        public bool RememberME { get; set; }

    }
}
