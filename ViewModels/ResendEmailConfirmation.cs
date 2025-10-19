using System.ComponentModel.DataAnnotations;


namespace TagerCom.ViewModels
{
    public class ResendEmailConfirmation
    {
        public int Id { get; set; }

        [Required]
        public string EmailOrUserName { get; set; } = string.Empty;

    }
}
