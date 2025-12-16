using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TagerCom.Areas.CustomerService.DTOs.Request;
using TagerCom.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static TagerCom.Models.Ticket;

namespace TagerCom.Areas.CustomerService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("CustomerService")]
    [Authorize(Roles = "CustomerService")]

    public class CsDashboard : ControllerBase
    {
        private readonly IRepository<Models.Store> _storeRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _orderItemRepo;
        private readonly IRepository<Ticket> _ticketRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public CsDashboard(IRepository<Models.Store> _storeRepo,
            IRepository<Order> _orderRepo, IRepository<OrderItem> _orderItemRepo)
        {
            this._storeRepo = _storeRepo;
            this._orderRepo = _orderRepo;
            this._orderItemRepo = _orderItemRepo;
            this._ticketRepo = _ticketRepo;
            this._userManager = _userManager;

        }

        private const int PageSize = 2;


        [HttpGet]
        public async Task<IActionResult> GetAllOrders(CustomerServiceOrdersRequest requestDTO)
        {


            var orders = _orderRepo.Query();


            // Filter   ----------------
            if (!string.IsNullOrEmpty(requestDTO.OrderNumber))
            {
                orders = orders.Where(o => o.Id == Guid.Parse(requestDTO.OrderNumber));
            }

            if (requestDTO.VendorId.HasValue)
            {
                orders = orders.Where(o => o.StoreId == requestDTO.VendorId);
            }


            if (!string.IsNullOrEmpty(requestDTO.CustomerEmail))
            {
                orders = orders.Where(o => o.Customer.Email == requestDTO.CustomerEmail);

            }
            if (requestDTO.Status.HasValue)
            {
                orders = orders.Where(o => o.OrderStatus == requestDTO.Status.Value);
            }



            if (requestDTO.ToDate.HasValue)
            {
                orders = orders.Where(o => o.CreatedAt == requestDTO.ToDate.Value);
            }


            // Sorting   ----------------



            var isDescending = requestDTO.Descending == true;

            switch (requestDTO.SortBy?.ToLower())
            {


                case "amount":
                    orders = isDescending ? orders.OrderByDescending(o => o.TotalAmount) : orders.OrderBy(o => o.TotalAmount);
                    break;

                case "status":
                    orders = isDescending
                        ? orders.OrderByDescending(o => o.OrderStatus)
                        : orders.OrderBy(o => o.OrderStatus);
                    break;


                case "date":
                default:
                    // Default: sort by CreatedAt
                    orders = isDescending
                        ? orders.OrderByDescending(o => o.CreatedAt)
                        : orders.OrderBy(o => o.CreatedAt);
                    break;

            }





            var currentPage = requestDTO.CurrentPage <= 0 ? 1 : requestDTO.CurrentPage;

            var totalCount = await orders.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var pageQuery = orders
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize);


            var items = await pageQuery
               .Select(o => new
               {
                   o.Id,
                   CustomerUsername = o.Customer.UserName,
                   CustomerEmail = o.Customer.Email,
                   VendorName = o.Store.StoreName,
                   o.OrderStatus,
                   o.TotalAmount,
                   o.CreatedAt
               })
               .ToListAsync();

            var result = new
            {
                currentPage,
                pageSize = PageSize,
                totalPages,
                totalCount,
                items
            };

            return Ok(result);

        }




        [HttpGet("{orderId:guid}")]

        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {

            var order = await _orderRepo.Query().Include(o => o.Customer)
                .Include(o => o.Store)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();



            var orderinfo = new
            {

                order.Id,
                Status = order.StatusHistory,
                order.TotalAmount,
                order.CreatedAt,


                Payment = order.Payment == null ? null : new
                {
                    order.Payment.Method,
                    order.Payment.PaymentStatus
                },


                Customer = new
                {
                    order.CustomerId,
                    Username = order.Customer?.UserName,
                    order.Customer.Email,
                    order.Customer.PhoneNumber

                },


                Store = order.Store == null ? null : new
                {
                    order.StoreId,
                    VendorEmail = order.Store.ApplicationUser?.Email,
                    VendorPhone = order.Store.ApplicationUser?.PhoneNumber,
                    VendorUserName = order.Store.ApplicationUser?.UserName

                },

                Items = order.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.ProductId,
                    oi.Quantity,
                    oi.Price,
                    oi.OrderId,
                }),


                StatusHistory = order.StatusHistory
                    .OrderBy(h => h.ChangedAt)
                    .Select(h => new
                    {
                        h.Status,
                        h.ChangedAt,
                        h.Description
                    })
            };

            return Ok(orderinfo);

        }


        [HttpPatch("{orderId:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
           Guid orderId,
           [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo
                .Query()
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // نسمح بس بالحالتين دول
            if (request.Status != OrderStatus.Cancelled &&
                request.Status != OrderStatus.Refunded)
            {
                return BadRequest("Status can only be Cancelled or Refunded.");
            }

            // تغيير حالة الأوردر
            order.OrderStatus = request.Status;

            // تسجيل تغيير الحالة + المشكلة (لو فيه)
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = request.Status,
                ChangedAt = DateTime.UtcNow,
                Description = request.IssueDescription
            });

            await _orderRepo.CommitAsync();

            return NoContent();
        }





        [HttpGet()]

        public async Task<IActionResult> GetAllTickets([FromQuery] GetTicketsDto ticketsDto)
        {
            const int pageSize = 3;

            IQueryable<Ticket> tickets = _ticketRepo.Query();



            // ======== Applying Filters ========

            if (ticketsDto.status.HasValue)
            {
                tickets = tickets.Where(t => t.status == ticketsDto.status.Value);
            }

            if (ticketsDto.type.HasValue)
            {
                tickets = tickets.Where(t => t.Type == ticketsDto.type.Value);
            }

            if (ticketsDto.priority.HasValue)
            {
                tickets = tickets.Where(t => t.priority == ticketsDto.priority.Value);
            }

            // ======== Applying Sorting ========

            var isDescending = ticketsDto.descending == true;

            switch (ticketsDto.SortBy?.ToLower())


            {

                case "priority":
                    tickets = isDescending ? tickets.OrderByDescending(t => t.priority)
                       : tickets.OrderBy(t => t.priority);
                    break;

                case "date":
                default:
                    tickets = isDescending
                       ? tickets.OrderByDescending(t => t.CreatedAt)
                          : tickets.OrderBy(t => t.CreatedAt);
                    break;


            }


            var count = await tickets.CountAsync();
            var pages = (int)Math.Ceiling(count / (double)pageSize);

            var data = await tickets.Skip((ticketsDto.CurrentPage - 1) * pageSize).Take(pageSize).
                Select(t => new
                {
                    t.Id,
                    t.Subject,
                    t.IssueDescription,
                    Status = t.status,
                    Priority = t.priority,
                    t.Type,
                    t.CreatedAt,
                    t.CustomerId,
                    CustomerName = t.Customer.UserName,

                    t.SupportId,
                    SupportName = t.Support != null ? t.Support.UserName : null
                })
                .ToListAsync();
            return Ok(new
            {
                pages,
                currentPage = ticketsDto.CurrentPage,
                data
            });
        }


        //  Get Ticket by id

        [HttpGet("{ticketId:guid}")]
        public async Task<IActionResult> GetSpecificTicket(Guid ticketId)
        {
            // ✅ get user from token using UserManager (جوا نفس الميثود)
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized("User not authenticated");

            var ticket = await _ticketRepo.Query()
                .Include(t => t.Customer)
                .Include(t => t.Support)
                .Include(t => t.Order)
                .Include(t => t.Updates)
                    .ThenInclude(u => u.Actor)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null) return NotFound("Ticket not found.");

            // العميل ما يشوفش internal updates (الدعم يشوف كله)
            bool isSupportSide = ticket.SupportId == currentUser.Id;
            var updates = isSupportSide
                ? ticket.Updates
                : ticket.Updates.Where(u => !u.IsInternal);

            return Ok(new
            {
                // Ticket details
                ticket.Id,
                ticket.Subject,
                Status = ticket.status.ToString(),
                Priority = ticket.priority.ToString(),
                Type = ticket.Type.ToString(),
                ticket.CreatedAt,

                // Customer info
                Customer = new
                {
                    ticket.CustomerId,
                    Name = ticket.Customer?.UserName,
                    Email = ticket.Customer?.Email
                },

                // Related order
                RelatedOrder = ticket.OrderId == null ? null : new
                {
                    ticket.OrderId,
                    // ضيف خصائص من Order لو موجودة عندك:
                    Number = ticket.Order?.OrderItems,
                    Status = ticket.Order?.OrderStatus,
                     Total = ticket.Order?.TotalAmount
                },

                // Issue description
                ticket.IssueDescription,

                // ✅ Attachments (صور/ملفات عامة للتذكرة)
                Attachments = ticket.Attachments,

                // Assigned agent
                AssignedAgent = ticket.SupportId == null ? null : new
                {
                    ticket.SupportId,
                    Name = ticket.Support?.UserName,
                    Email = ticket.Support?.Email
                },

                // Comments/Updates history
                CommentsAndUpdatesHistory = updates
                    .OrderBy(u => u.CreatedAt)
                    .Select(u => new
                    {
                        u.Id,
                        u.CreatedAt,
                        u.IsInternal,
                        u.Message,

                        // لو ضايفهم في TicketUpdate
                        u.OldStatus,
                        u.NewStatus,
                        u.OldSupportId,
                        u.NewSupportId,

                        Actor = new
                        {
                            u.ActorId,
                            Name = u.Actor?.UserName,
                            Email = u.Actor?.Email
                        }
                    })
            });



            //=====================================================================//


        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDTO form)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            var ticket = new Ticket
            {
                CustomerId = user.Id,
                OrderId = form.OrderId,
                Type = form.Type,
                priority = form.Priority,
                Subject = form.Subject,
                IssueDescription = form.IssueDescription,
                Attachments = new List<string>(),
                Updates = new List<TicketUpdate>()
            };

            var ticketFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "tickets", ticket.Id.ToString()
            );

            try
            {
                if (form.Attachments is not null && form.Attachments.Count > 0)
                {
                    foreach (var file in form.Attachments)
                    {
                        var fileName = await SaveTicketImageAsync(file, ticketFolder);
                        ticket.Attachments.Add($"/uploads/tickets/{ticket.Id}/{fileName}");
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            await _ticketRepo.AddAsync(ticket);
            await _ticketRepo.CommitAsync();

            return Ok(new
            {
                customerId = ticket.CustomerId,
                orderId = ticket.OrderId,
                type = ticket.Type,
                priority = ticket.priority,
                subject = ticket.Subject,
                description = ticket.IssueDescription,
                attachments = ticket.Attachments
            });
        }





        public async Task<string> SaveTicketImageAsync(IFormFile file, string folderPath)
        {
            Directory.CreateDirectory(folderPath);

            // Validate extension ------------------------------------------------------------
            var ext = Path.GetExtension(file.FileName);
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext.ToLowerInvariant()))
                throw new InvalidOperationException("Invalid image type.");
            // -------------------------------

            // Max size  --------------------------------------------------
            if (file.Length > 20 * 1024 * 1024)
                throw new InvalidOperationException("File too large.");
            // ----------------------------------------

            // Getting name ----------------------------
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }




        [HttpPatch("{ticketId:guid}")]
        public async Task<IActionResult> UpdateTicket(Guid ticketId, [FromBody] UpdateTicketDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            var ticket = await _ticketRepo.Query()
                .Include(t => t.Updates)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket is null) return NotFound("Ticket not found.");

            // صلاحية بسيطة: يا صاحب التيكت يا الـ Support المعيّن
            var isOwnerCustomer = ticket.CustomerId == user.Id;
            var isAssignedSupport = ticket.SupportId != null && ticket.SupportId == user.Id;

            if (!isOwnerCustomer && !isAssignedSupport)
                return Forbid("You are not allowed to update this ticket.");

            // Change status (Open -> InProgress -> Resolved فقط)
            if (dto.Status.HasValue && dto.Status.Value != ticket.status)
            {
                var ok =
                    (ticket.status == TicketStatus.Open && dto.Status.Value == TicketStatus.InProgress) ||
                    (ticket.status == TicketStatus.InProgress && dto.Status.Value == TicketStatus.Resolved);

                if (!ok) return BadRequest("Invalid status transition.");

                ticket.status = dto.Status.Value;
            }

            // Change priority
            if (dto.Priority.HasValue && dto.Priority.Value != ticket.priority)
                ticket.priority = dto.Priority.Value;

            // Assign to agent
            if (!string.IsNullOrWhiteSpace(dto.SupportId) && dto.SupportId != ticket.SupportId)
                ticket.SupportId = dto.SupportId;

            // Add note (internal or not)
            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                // منع العميل من internal notes
                if (dto.IsInternal && isOwnerCustomer && !isAssignedSupport)
                    return Forbid("Customer cannot add internal notes.");

                ticket.Updates.Add(new TicketUpdate
                {
                    TicketId = ticket.Id,
                    ActorId = user.Id,
                    Message = dto.Note,
                    IsInternal = dto.IsInternal
                });
            }

            await _ticketRepo.CommitAsync();

            var lastUpdate = ticket.Updates.LastOrDefault();

            return Ok(new
            {
                ticket.Id,
                status = ticket.status,
                priority = ticket.priority,
                supportId = ticket.SupportId,
                updatesCount = ticket.Updates.Count,
                lastUpdate = lastUpdate == null ? null : new
                {
                    message = lastUpdate.Message,
                    actorId = lastUpdate.ActorId,
                    isInternal = lastUpdate.IsInternal
                }
            });
        }



        [HttpPost("{ticketId:guid}/comments")]
        public async Task<IActionResult> AddComment(Guid ticketId, [FromBody] AddCommentDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            var ticket = await _ticketRepo.Query()
                .Include(t => t.Updates)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket is null) return NotFound("Ticket not found.");

            // صلاحية بسيطة: owner أو assigned support
            var isOwnerCustomer = ticket.CustomerId == user.Id;
            var isAssignedSupport = ticket.SupportId != null && ticket.SupportId == user.Id;

            if (!isOwnerCustomer && !isAssignedSupport)
                return Forbid("You are not allowed to comment on this ticket.");

            if (string.IsNullOrWhiteSpace(dto.Comment))
                return BadRequest("Comment is required.");

            // ✅ لو فيه attachments (paths) ضيفها على ticket.Attachments
            if (dto.Attachments is not null && dto.Attachments.Count > 0)
            {
                foreach (var path in dto.Attachments)
                {
                    if (!string.IsNullOrWhiteSpace(path))
                        ticket.Attachments.Add(path);
                }
            }

            // ✅ سجل التعليق في TicketUpdate
            var update = new TicketUpdate
            {
                TicketId = ticket.Id,
                ActorId = user.Id,
                Message = dto.Comment,
                IsInternal = false,
                CreatedAt = DateTime.UtcNow
            };
            ticket.Updates.Add(update);

            await _ticketRepo.CommitAsync();

            return Ok(new
            {
                ticket.Id,
                updatesCount = ticket.Updates.Count,
                lastUpdate = new
                {
                    message = update.Message,
                    actorId = update.ActorId,
                    isInternal = update.IsInternal,
                    createdAt = update.CreatedAt
                },
                attachments = ticket.Attachments
            });
        }

        [HttpPost("{ticketId:guid}/close")]
        public async Task<IActionResult> CloseTicket(Guid ticketId, [FromBody] CloseTicketDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            var ticket = await _ticketRepo.Query()
                .Include(t => t.Updates)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket is null) return NotFound("Ticket not found.");

            // صلاحية بسيطة: owner أو assigned support
            var isOwnerCustomer = ticket.CustomerId == user.Id;
            var isAssignedSupport = ticket.SupportId != null && ticket.SupportId == user.Id;

            if (!isOwnerCustomer && !isAssignedSupport)
                return Forbid("You are not allowed to close this ticket.");

            if (string.IsNullOrWhiteSpace(dto.ResolutionNotes))
                return BadRequest("ResolutionNotes is required.");

            // لو مقفولة بالفعل
            if (ticket.status == Ticket.TicketStatus.Resolved)
                return BadRequest("Ticket is already resolved.");

            // Validate rating لو موجود
            if (dto.SatisfactionRating.HasValue &&
                (dto.SatisfactionRating.Value < 1 || dto.SatisfactionRating.Value > 5))
                return BadRequest("SatisfactionRating must be between 1 and 5.");

            var oldStatus = ticket.status;

            // Close
            ticket.status = Ticket.TicketStatus.Resolved;
            ticket.ResolutionNotes = dto.ResolutionNotes;

            // Survey
            ticket.SatisfactionRating = dto.SatisfactionRating;

            // Archive
            ticket.IsArchived = dto.Archive;

            // Update log
            ticket.Updates.Add(new TicketUpdate
            {
                TicketId = ticket.Id,
                ActorId = user.Id,
                Message = $"Closed ticket. Notes: {dto.ResolutionNotes}",
                IsInternal = false,
                OldStatus = oldStatus,
                NewStatus = Ticket.TicketStatus.Resolved,
                CreatedAt = DateTime.UtcNow
            });

            await _ticketRepo.CommitAsync();

            return Ok(new
            {
                ticket.Id,
                status = ticket.status,
                isArchived = ticket.IsArchived,
                resolutionNotes = ticket.ResolutionNotes,
                satisfactionRating = ticket.SatisfactionRating,
            });
        }


    }



}






















