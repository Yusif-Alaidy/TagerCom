namespace TagerCom.Areas.CustomerService.DTOs.Request
{
    public class GetTicketsDto
    {
        //-----------------  Filter  -----------------

        public Ticket.TicketStatus? status {  get; set; }
        public Ticket.Priority? priority { get; set; }
        public Ticket.TicketType? type { get; set; }



        //-----------------  SortBy  -----------------

        public string? SortBy { get; set; } = "date";
        public bool? descending { get; set; }


        //-----------------  Pagniation  -----------------

        public int CurrentPage { get; set; } = 1;


    }
}
