using System.ComponentModel.DataAnnotations;

namespace TagerCom.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }

        [Required]
        public string EmailOrUserName { get; set; } =string.Empty;

        


        [Required,DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        public bool RememberME { get; set; }

    }
}
