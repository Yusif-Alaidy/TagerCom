namespace TagerCom.Models
   
{
    public class Complaint
    {
        public enum ComplaintStatus { Open, InProgress, Resolved }
        public enum ComplaintPriority { Low, Medium, High }
        public enum ComplaintType
        {
            OrderIssue,
            PaymentProblem,
            ShippingDelay,
            ProductQuality,
            RefundRequest,
            GeneralInquiry
        }

        public Guid Id { get; set; } = Guid.NewGuid();

        // Customer
        public string CustomerId { get; set; } = null!;
        public ApplicationUser Customer { get; set; } = null!;

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        // Vendor filter 
        public Guid? StoreId { get; set; }
        public Store Store { get; set; }
        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public ComplaintType Type { get; set; } = ComplaintType.OrderIssue;
        public ComplaintPriority Priority { get; set; } = ComplaintPriority.Medium;
        public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
        public int OverdueByMinutes { get; set; }     
        public string OverdueByText { get; set; } = ""; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        public bool IsEscalated { get; set; } = false;

        public string? EscalatedById { get; set; }          // مين صعّدها
        public string? EscalatedToManagerId { get; set; }   // المدير

        public int? UrgencyLevel { get; set; }              // 1..5
        public DateTime? EscalationDeadline { get; set; }   // deadline
        public DateTime? EscalatedAt { get; set; }          // وقت التصعيد

        public bool ManagementNotified { get; set; } = false;
        public DateTime? ManagementNotifiedAt { get; set; }




    }
}
