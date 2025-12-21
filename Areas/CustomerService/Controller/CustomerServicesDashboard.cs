using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using TagerCom.Areas.CustomerService.DTOs.Request;
using TagerCom.Areas.CustomerService.DTOs.Response;
using TagerCom.Models;
using static TagerCom.Models.Complaint;
using static TagerCom.Models.Ticket;

namespace TagerCom.Areas.CustomerService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Area("CustomerService")]
    [Authorize(Roles = "CustomerService")]
    public class CustomerServicesDashboard : ControllerBase
    {
        #region Dependencies & Constructor

        private readonly IRepository<Models.Store> _storeRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<OrderItem> _orderItemRepo;
        private readonly IRepository<Ticket> _ticketRepo;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Complaint> _complaintRepo;
        private readonly IRepository<TicketUpdate> _ticketupdateRepo;

        private const int PageSize = 2;

        public CustomerServicesDashboard(
            IRepository<Models.Store> _storeRepo,
            IRepository<Order> _orderRepo,
            IRepository<OrderItem> _orderItemRepo,
            IRepository<Ticket> _ticketRepo,
            IRepository<Complaint> _complaintRepo,
            UserManager<ApplicationUser> _userManager,
            IRepository<TicketUpdate> ticketupdateRepo)
        {
            this._storeRepo = _storeRepo;
            this._orderRepo = _orderRepo;
            this._orderItemRepo = _orderItemRepo;
            this._ticketRepo = _ticketRepo;
            this._userManager = _userManager;
            this._complaintRepo = _complaintRepo;
            this._ticketupdateRepo = ticketupdateRepo;
        }

        #endregion

        #region GetAllOrders

        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] CustomerServiceOrdersRequest requestDTO)
        {
            var orders = _orderRepo.Query();

            // Filters
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

            if (requestDTO.FromDate.HasValue)
            {
                var from = requestDTO.FromDate.Value.Date;
                orders = orders.Where(o => o.CreatedAt >= from);
            }

            if (requestDTO.ToDate.HasValue)
            {
                var toExclusive = requestDTO.ToDate.Value.Date.AddDays(1);
                orders = orders.Where(o => o.CreatedAt < toExclusive);
            }

            // Sorting
            var isDescending = requestDTO.Descending == true;

            switch (requestDTO.SortBy?.ToLower())
            {
                case "amount":
                    orders = isDescending
                        ? orders.OrderByDescending(o => o.TotalAmount)
                        : orders.OrderBy(o => o.TotalAmount);
                    break;

                case "status":
                    orders = isDescending
                        ? orders.OrderByDescending(o => o.OrderStatus)
                        : orders.OrderBy(o => o.OrderStatus);
                    break;

                case "date":
                default:
                    orders = isDescending
                        ? orders.OrderByDescending(o => o.CreatedAt)
                        : orders.OrderBy(o => o.CreatedAt);
                    break;
            }

            // Pagination
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

        #endregion

        #region GetOrderDetails

        [HttpGet("orders/{orderId:guid}")]
        public async Task<IActionResult> GetOrderDetails(Guid orderId)
        {
            var order = await _orderRepo.Query()
                .Include(o => o.Customer)
                .Include(o => o.Store)
                    .ThenInclude(s => s.ApplicationUser)
                .Include(o => o.Payment)
                .Include(o => o.OrderItems)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            var computedTotal = order.OrderItems?.Sum(oi => oi.Quantity * oi.Price) ?? 0m;

            var orderinfo = new
            {
                order.Id,
                Status = order.OrderStatus,
                TotalAmount = order.TotalAmount ?? computedTotal,
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
                    Email = order.Customer?.Email,
                    PhoneNumber = order.Customer?.PhoneNumber
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
                    .Select(h => new { h.Status, h.ChangedAt, h.Description })
            };

            return Ok(orderinfo);
        }

        #endregion

        #region UpdateOrderStatus

        [HttpPatch("status/{orderId:guid}")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _orderRepo
                .Query()
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // Only allow these statuses
            if (request.Status != OrderStatus.Cancelled &&
                request.Status != OrderStatus.Refunded)
            {
                return BadRequest("Status can only be Cancelled or Refunded.");
            }

            // Update status
            order.OrderStatus = request.Status;

            // Log status change with optional issue description
            order.StatusHistory.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = request.Status,
                ChangedAt = DateTime.UtcNow,
                Description = request.IssueDescription
            });

            await _orderRepo.CommitAsync();

            return Ok(new
            {
                order.Id,
                order.OrderStatus,
                StatusHistory = order.StatusHistory
                    .OrderByDescending(h => h.ChangedAt)
                    .Select(h => new { h.Status, h.ChangedAt, h.Description })
            });
        }

        #endregion

        #region GetOrdersByVendor

        [HttpGet("orders/vendor/{vendorId:guid}")]
        public async Task<IActionResult> GetOrdersByVendor(
            Guid vendorId,
            [FromQuery] CustomerServiceOrdersRequest requestDTO,
            [FromQuery] bool problemOnly = false)
        {
            // Base query (vendor orders)
            var orders = _orderRepo.Query()
                .AsNoTracking()
                .Where(o => o.StoreId == vendorId);

            // Filters
            if (!string.IsNullOrWhiteSpace(requestDTO.OrderNumber))
            {
                if (Guid.TryParse(requestDTO.OrderNumber, out var orderGuid))
                    orders = orders.Where(o => o.Id == orderGuid);
                else
                    return BadRequest("Invalid OrderNumber format.");
            }

            if (!string.IsNullOrWhiteSpace(requestDTO.CustomerEmail))
                orders = orders.Where(o => o.Customer.Email == requestDTO.CustomerEmail);

            if (requestDTO.Status.HasValue)
                orders = orders.Where(o => o.OrderStatus == requestDTO.Status.Value);

            if (requestDTO.FromDate.HasValue)
            {
                var from = requestDTO.FromDate.Value.Date;
                orders = orders.Where(o => o.CreatedAt >= from);
            }

            if (requestDTO.ToDate.HasValue)
            {
                var toExclusive = requestDTO.ToDate.Value.Date.AddDays(1);
                orders = orders.Where(o => o.CreatedAt < toExclusive);
            }

            // Problem filter:
            // Has complaint OR has ticket OR status is Cancelled/Refunded
            if (problemOnly)
            {
                var complaintOrderIds = _complaintRepo.Query()
                    .Where(c => c.StoreId == vendorId)
                    .Select(c => c.OrderId);

                var ticketOrderIds = _ticketRepo.Query()
                    .Where(t => t.OrderId != null && t.Order.StoreId == vendorId)
                    .Select(t => t.OrderId!.Value);

                orders = orders.Where(o =>
                    complaintOrderIds.Contains(o.Id) ||
                    ticketOrderIds.Contains(o.Id) ||
                    o.OrderStatus == OrderStatus.Cancelled ||
                    o.OrderStatus == OrderStatus.Refunded
                );
            }

            // Sorting
            var isDescending = requestDTO.Descending == true;

            switch (requestDTO.SortBy?.ToLower())
            {
                case "amount":
                    orders = isDescending ? orders.OrderByDescending(o => o.TotalAmount) : orders.OrderBy(o => o.TotalAmount);
                    break;

                case "status":
                    orders = isDescending ? orders.OrderByDescending(o => o.OrderStatus) : orders.OrderBy(o => o.OrderStatus);
                    break;

                case "date":
                default:
                    orders = isDescending ? orders.OrderByDescending(o => o.CreatedAt) : orders.OrderBy(o => o.CreatedAt);
                    break;
            }

            // Metrics (based on filtered query before pagination)
            var baseOrderIds = orders.Select(o => o.Id);

            var complaintsCount = await (
                from c in _complaintRepo.Query()
                join o in orders on c.OrderId equals o.Id
                select c.Id
            ).CountAsync();

            var ticketsCount = await _ticketRepo.Query()
                .CountAsync(t => t.OrderId != null && baseOrderIds.Contains(t.OrderId.Value));

            var metrics = await orders
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    totalOrders = g.Count(),
                    cancelledOrders = g.Count(x => x.OrderStatus == OrderStatus.Cancelled),
                    refundedOrders = g.Count(x => x.OrderStatus == OrderStatus.Refunded),
                    totalAmount = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m,
                    avgOrderValue = (g.Sum(x => (decimal?)x.TotalAmount) ?? 0m) / (g.Count() == 0 ? 1 : g.Count())
                })
                .FirstOrDefaultAsync();

            metrics ??= new
            {
                totalOrders = 0,
                cancelledOrders = 0,
                refundedOrders = 0,
                totalAmount = 0m,
                avgOrderValue = 0m
            };

            var problemOrdersCount = await orders.CountAsync(o =>
                _complaintRepo.Query().Any(c => c.OrderId == o.Id) ||
                _ticketRepo.Query().Any(t => t.OrderId == o.Id) ||
                o.OrderStatus == OrderStatus.Cancelled ||
                o.OrderStatus == OrderStatus.Refunded
            );

            // Pagination
            var currentPage = requestDTO.CurrentPage <= 0 ? 1 : requestDTO.CurrentPage;

            var totalCount = await orders.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            var items = await orders
                .Skip((currentPage - 1) * PageSize)
                .Take(PageSize)
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

            return Ok(new
            {
                paging = new
                {
                    currentPage,
                    pageSize = PageSize,
                    totalPages,
                    totalCount,
                    hasNext = currentPage < totalPages,
                    hasPrevious = currentPage > 1
                },

                vendorId,
                filters = new
                {
                    requestDTO.FromDate,
                    requestDTO.ToDate,
                    requestDTO.Status,
                    requestDTO.CustomerEmail,
                    problemOnly
                },

                metrics = new
                {
                    metrics.totalOrders,
                    metrics.totalAmount,
                    metrics.avgOrderValue,
                    metrics.cancelledOrders,
                    metrics.refundedOrders,
                    complaintsCount,
                    ticketsCount,
                    problemOrdersCount
                },

                items
            });
        }

        #endregion

        #region GetAllTickets

        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets([FromQuery] GetTicketsDto ticketsDto)
        {
            const int pageSize = 3;

            IQueryable<Ticket> tickets = _ticketRepo.Query();

            // Filters
            if (ticketsDto.status.HasValue)
                tickets = tickets.Where(t => t.status == ticketsDto.status.Value);

            if (ticketsDto.type.HasValue)
                tickets = tickets.Where(t => t.Type == ticketsDto.type.Value);

            if (ticketsDto.priority.HasValue)
                tickets = tickets.Where(t => t.priority == ticketsDto.priority.Value);

            // Sorting
            var isDescending = ticketsDto.descending == true;

            switch (ticketsDto.SortBy?.ToLower())
            {
                case "priority":
                    tickets = isDescending ? tickets.OrderByDescending(t => t.priority) : tickets.OrderBy(t => t.priority);
                    break;

                case "date":
                default:
                    tickets = isDescending ? tickets.OrderByDescending(t => t.CreatedAt) : tickets.OrderBy(t => t.CreatedAt);
                    break;
            }

            var count = await tickets.CountAsync();
            var pages = (int)Math.Ceiling(count / (double)pageSize);

            var data = await tickets
                .Skip((ticketsDto.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
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

        #endregion

        #region GetSpecificTicket

        [HttpGet("tickets/{ticketId:guid}")]
        public async Task<IActionResult> GetSpecificTicket(Guid ticketId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized("User not authenticated");

            var ticket = await _ticketRepo.Query()
                .Include(t => t.Customer)
                .Include(t => t.Support)
                .Include(t => t.Order)
                .Include(t => t.Updates)
                    .ThenInclude(u => u.Actor)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket == null)
                return NotFound("Ticket not found.");

            // Return full update history (no filtering)
            var updates = ticket.Updates ?? new List<TicketUpdate>();

            List<TicketUpdateDto> history;

            if (updates.Any())
            {
                history = updates
                    .OrderBy(u => u.CreatedAt)
                    .Select(u => new TicketUpdateDto
                    {
                        Id = u.Id,
                        CreatedAt = u.CreatedAt,
                        IsInternal = u.IsInternal,
                        Message = u.Message,

                        OldStatus = u.OldStatus,
                        NewStatus = u.NewStatus,
                        OldSupportId = u.OldSupportId,

                        Actor = u.Actor == null ? null : new ActorDto
                        {
                            ActorId = u.ActorId,
                            Name = u.Actor.UserName,
                            Email = u.Actor.Email
                        }
                    })
                    .ToList();
            }
            else
            {
                // Simple placeholder when no updates exist
                history = new List<TicketUpdateDto>
                {
                    new TicketUpdateDto
                    {
                        Id = Guid.Empty,
                        CreatedAt = ticket.CreatedAt,
                        IsInternal = false,
                        Message = "No updates yet",
                        OldStatus = null,
                        NewStatus = null,
                        OldSupportId = null,
                        NewSupportId = null,
                        Actor = null
                    }
                };
            }

            return Ok(new
            {
                ticket.Id,
                ticket.Subject,
                Status = ticket.status.ToString(),
                Priority = ticket.priority.ToString(),
                Type = ticket.Type.ToString(),
                ticket.CreatedAt,

                Customer = new
                {
                    ticket.CustomerId,
                    Name = ticket.Customer?.UserName,
                    Email = ticket.Customer?.Email
                },

                RelatedOrder = ticket.OrderId == null ? null : new
                {
                    ticket.OrderId,
                    Number = ticket.Order?.OrderItems,
                    Status = ticket.Order?.OrderStatus,
                    Total = ticket.Order?.TotalAmount
                },

                ticket.IssueDescription,
                Attachments = ticket.Attachments,

                AssignedAgent = ticket.SupportId == null ? null : new
                {
                    ticket.SupportId,
                    Name = ticket.Support?.UserName,
                    Email = ticket.Support?.Email
                },

                CommentsAndUpdatesHistory = history
            });
        }

        #endregion

        #region CreateTicket

        [HttpPost("CreateTicket")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> CreateTicket([FromForm] CreateTicketDTO form)
        {
            // Current user (support)
            var supportUser = await _userManager.GetUserAsync(User);
            if (supportUser is null) return Unauthorized("User not authenticated");

            // Get order to read CustomerId
            var order = await _orderRepo.GetOneAsync(o => o.Id == form.OrderId, tracked: false);
            if (order is null)
                return BadRequest("Order not found");

            // Create ticket id (no DB default)
            var ticketId = Guid.NewGuid();

            var ticket = new Ticket
            {
                Id = ticketId,
                SupportId = supportUser.Id,
                CustomerId = order.CustomerId,

                OrderId = form.OrderId,
                Type = form.Type,
                priority = form.Priority,
                Subject = form.Subject,
                IssueDescription = form.IssueDescription,

                Attachments = new List<string>(),
                Updates = new List<TicketUpdate>()
            };

            // Ticket folder (Id exists)
            var ticketFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "tickets", ticket.Id.ToString()
            );

            try
            {
                // Save attachments if provided
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
                supportId = ticket.SupportId,
                customerId = ticket.CustomerId,
                orderId = ticket.OrderId,
                type = ticket.Type,
                priority = ticket.priority,
                subject = ticket.Subject,
                description = ticket.IssueDescription,
                attachments = ticket.Attachments
            });
        }

        #endregion

        #region SaveTicketImageAsync

        private async Task<string> SaveTicketImageAsync(IFormFile file, string folderPath)
        {
            Directory.CreateDirectory(folderPath);

            // Validate extension
            var ext = Path.GetExtension(file.FileName);
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext.ToLowerInvariant()))
                throw new InvalidOperationException("Invalid image type.");

            // Max size
            if (file.Length > 20 * 1024 * 1024)
                throw new InvalidOperationException("File too large.");

            // Build file name
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        #endregion

        #region UpdateTicket

        [HttpPatch("Ticket/{ticketId:guid}")]
        public async Task<IActionResult> UpdateTicket(Guid ticketId, [FromBody] UpdateTicketDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            var ticket = await _ticketRepo.Query().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket is null) return NotFound("Ticket not found.");

            // Auto-assign
            if (string.IsNullOrWhiteSpace(ticket.SupportId))
                ticket.SupportId = user.Id;
            else if (ticket.SupportId != user.Id)
                return Forbid("Ticket is assigned to another support agent.");

            // Snapshot before changes
            var oldStatus = ticket.status;
            var oldSupportId = ticket.SupportId;

            bool statusChanged = false;
            bool supportChanged = false;

            // Status transitions
            if (dto.Status.HasValue && dto.Status.Value != ticket.status)
            {
                var ok =
                    (ticket.status == TicketStatus.Open && dto.Status.Value == TicketStatus.InProgress) ||
                    (ticket.status == TicketStatus.InProgress && dto.Status.Value == TicketStatus.Resolved);

                if (!ok) return BadRequest("Invalid status transition.");

                ticket.status = dto.Status.Value;
                statusChanged = true;
            }

            // Priority
            if (dto.Priority.HasValue && dto.Priority.Value != ticket.priority)
                ticket.priority = dto.Priority.Value;

            // Re-assign
            if (!string.IsNullOrWhiteSpace(dto.SupportId) && dto.SupportId != ticket.SupportId)
            {
                var supportTarget = await _userManager.FindByIdAsync(dto.SupportId);
                if (supportTarget is null) return BadRequest("Support user not found.");

                ticket.SupportId = dto.SupportId;
                supportChanged = true;
            }

            // Create update row if there is a message or any change
            if (!string.IsNullOrWhiteSpace(dto.Message) || statusChanged || supportChanged)
            {
                var msg = (dto.Message ?? "").Trim();

                if (string.IsNullOrWhiteSpace(msg))
                {
                    if (statusChanged) msg = $"Status changed: {oldStatus} -> {ticket.status}";
                    if (supportChanged) msg += (msg.Length > 0 ? " | " : "") + $"Support changed: {oldSupportId} -> {ticket.SupportId}";
                }

                await _ticketupdateRepo.AddAsync(new TicketUpdate
                {
                    TicketId = ticket.Id,
                    ActorId = user.Id,
                    Message = msg,
                    IsInternal = dto.IsInternal,
                    CreatedAt = DateTime.UtcNow,

                    OldStatus = statusChanged ? oldStatus : null,
                    NewStatus = statusChanged ? ticket.status : null,
                    OldSupportId = supportChanged ? oldSupportId : null,
                });
            }

            await _ticketRepo.CommitAsync();        // Save ticket changes
            await _ticketupdateRepo.CommitAsync();  // Save update row

            return Ok(new { ticket.Id, status = ticket.status, supportId = ticket.SupportId });
        }

        #endregion

        #region AddComment

        [HttpPost("comments/{ticketId:guid}")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> AddComment(Guid ticketId, [FromForm] AddCommentDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            // Read only required fields
            var ticket = await _ticketRepo.Query()
                .AsNoTracking()
                .Select(t => new { t.Id, t.CustomerId, t.SupportId })
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket is null) return NotFound("Ticket not found.");

            var isOwnerCustomer = ticket.CustomerId == user.Id;
            var isAssignedSupport = ticket.SupportId != null && ticket.SupportId == user.Id;

            if (!isOwnerCustomer && !isAssignedSupport)
                return Forbid("You are not allowed to comment on this ticket.");

            var comment = (dto.Comment ?? "").Trim();
            if (string.IsNullOrWhiteSpace(comment))
                return BadRequest("Comment is required.");

            // Files folder
            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot", "uploads", "tickets", ticketId.ToString()
            );

            var savedPaths = new List<string>();

            if (dto.Attachments is not null && dto.Attachments.Count > 0)
            {
                foreach (var file in dto.Attachments)
                {
                    if (file == null || file.Length == 0) continue;

                    try
                    {
                        var fileName = await SaveTicketImageAsync(file, folderPath);
                        var relativePath = $"/uploads/tickets/{ticketId}/{fileName}";
                        savedPaths.Add(relativePath);
                    }
                    catch (InvalidOperationException ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }

            // Save comment as a TicketUpdate row
            var update = new TicketUpdate
            {
                TicketId = ticket.Id,
                ActorId = user.Id,
                Message = comment,
                IsInternal = false,
                CreatedAt = DateTime.UtcNow,
                Attachments = savedPaths
            };

            await _ticketupdateRepo.AddAsync(update);
            await _ticketupdateRepo.CommitAsync();

            return Ok(new
            {
                comment = update.Message,
                attachments = update.Attachments
            });
        }

        #endregion

        #region CloseTicket

        [HttpPost("close/{ticketId:guid}")]
        public async Task<IActionResult> CloseTicket(Guid ticketId, [FromBody] CloseTicketDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            // Tracking ticket (will be updated)
            var ticket = await _ticketRepo.Query()
                .FirstOrDefaultAsync(t => t.Id == ticketId);

            if (ticket is null) return NotFound("Ticket not found.");

            // Permission: owner customer OR assigned support
            var isOwnerCustomer = ticket.CustomerId == user.Id;
            var isAssignedSupport = ticket.SupportId != null && ticket.SupportId == user.Id;

            if (!isOwnerCustomer && !isAssignedSupport)
                return Forbid("You are not allowed to close this ticket.");

            if (string.IsNullOrWhiteSpace(dto.ResolutionNotes))
                return BadRequest("ResolutionNotes is required.");

            if (ticket.status == TicketStatus.Resolved)
                return BadRequest("Ticket is already resolved.");

            if (dto.SatisfactionRating.HasValue &&
                (dto.SatisfactionRating.Value < 1 || dto.SatisfactionRating.Value > 5))
                return BadRequest("SatisfactionRating must be between 1 and 5.");

            var oldStatus = ticket.status;

            // Close ticket
            ticket.status = TicketStatus.Resolved;
            ticket.ResolutionNotes = dto.ResolutionNotes;

            // Optional survey
            ticket.SatisfactionRating = dto.SatisfactionRating;

            // Optional archive
            ticket.IsArchived = dto.Archive;

            // Log update
            var update = new TicketUpdate
            {
                TicketId = ticket.Id,
                ActorId = user.Id,
                Message = $"Closed ticket. Notes: {dto.ResolutionNotes}",
                IsInternal = false,
                OldStatus = oldStatus,
                NewStatus = TicketStatus.Resolved,
                CreatedAt = DateTime.UtcNow
            };

            await _ticketupdateRepo.AddAsync(update);

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

        #endregion

        #region GetAllComplaint

        [HttpGet("complaints")]
        public async Task<IActionResult> GetAllComplaint([FromQuery] GetComplaintsQueryDTO getComplaints)
        {
            var now = DateTime.UtcNow;

            int pageSize = PageSize;
            int page = getComplaints.CurrentPage <= 0 ? 1 : getComplaints.CurrentPage;

            var complaints = _complaintRepo.Query().AsNoTracking();

            // Filters
            if (getComplaints.Status.HasValue)
                complaints = complaints.Where(c => c.Status == getComplaints.Status.Value);

            if (getComplaints.VendorId.HasValue)
                complaints = complaints.Where(c => c.StoreId == getComplaints.VendorId.Value);

            if (getComplaints.Type.HasValue)
                complaints = complaints.Where(c => c.Type == getComplaints.Type.Value);

            if (getComplaints.HighPriority == true)
                complaints = complaints.Where(c => c.Priority == ComplaintPriority.High);

            // Overdue only (before pagination)
            if (getComplaints.OverdueOnly == true)
            {
                complaints = complaints.Where(c =>
                    c.Status != ComplaintStatus.Resolved &&
                    now > c.CreatedAt.AddHours(
                        c.Priority == ComplaintPriority.High ? 24 :
                        c.Priority == ComplaintPriority.Medium ? 48 : 72
                    )
                );

                // Most overdue first (earliest due date)
                complaints = complaints.OrderBy(c => c.CreatedAt.AddHours(
                    c.Priority == ComplaintPriority.High ? 24 :
                    c.Priority == ComplaintPriority.Medium ? 48 : 72
                ));
            }
            else
            {
                // Default sort: newest first
                complaints = complaints.OrderByDescending(c => c.CreatedAt);
            }

            // Total after filters
            var total = await complaints.CountAsync();
            var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

            // Pagination + projection
            var items = await complaints
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ComplaintListItemDTO
                {
                    Id = c.Id,
                    OrderId = c.OrderId,
                    VendorId = c.StoreId,
                    Subject = c.Subject,
                    Type = c.Type,
                    Priority = c.Priority,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,

                    DueAt = c.CreatedAt.AddHours(
                        c.Priority == ComplaintPriority.High ? 24 :
                        c.Priority == ComplaintPriority.Medium ? 48 : 72
                    ),

                    SlaRemainingMinutes = 0,
                    IsOverdue = false,
                    OverdueByMinutes = 0,
                    OverdueByText = ""
                })
                .ToListAsync();

            // SLA + overdue calculation
            foreach (var it in items)
            {
                it.IsOverdue = (now > it.DueAt) && it.Status != ComplaintStatus.Resolved;

                var remaining = (int)Math.Ceiling((it.DueAt - now).TotalMinutes);
                it.SlaRemainingMinutes = remaining < 0 ? 0 : remaining;

                if (it.IsOverdue)
                {
                    it.OverdueByMinutes = (int)Math.Ceiling((now - it.DueAt).TotalMinutes);
                    it.OverdueByText = FormatOverdue(it.OverdueByMinutes);
                }
                else
                {
                    it.OverdueByMinutes = 0;
                    it.OverdueByText = "";
                }
            }

            // Optional: order by the actual overdue minutes
            if (getComplaints.OverdueOnly == true)
                items = items.OrderByDescending(x => x.OverdueByMinutes).ToList();

            return Ok(new
            {
                currentPage = page,
                pageSize,
                total,
                totalPages,
                hasNext = page < totalPages,
                hasPrevious = page > 1,
                items
            });
        }

        #endregion

        #region FormatOverdue

        private static string FormatOverdue(int minutes)
        {
            if (minutes <= 0) return "";

            var ts = TimeSpan.FromMinutes(minutes);
            var days = (int)ts.TotalDays;
            var hours = ts.Hours;
            var mins = ts.Minutes;

            if (days > 0) return $"{days}d {hours}h {mins}m";
            if (hours > 0) return $"{hours}h {mins}m";
            return $"{mins}m";
        }

        #endregion

        #region EscalateComplaint

        [HttpPost("escalate/{complaintId:guid}")]
        public async Task<IActionResult> EscalateComplaint(Guid complaintId, [FromBody] EscalateComplaintDTO dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return Unauthorized("User not authenticated");

            var complaint = await _complaintRepo.Query()
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint is null) return NotFound("Complaint not found.");

            // Customer cannot escalate complaints
            var isOwnerCustomer = complaint.CustomerId == user.Id;
            if (isOwnerCustomer) return Forbid("Customer cannot escalate complaints.");

            if (complaint.IsEscalated)
                return BadRequest("Complaint already escalated.");

            // Validate manager
            if (string.IsNullOrWhiteSpace(dto.ManagerId))
                return BadRequest("ManagerId is required.");

            var manager = await _userManager.FindByIdAsync(dto.ManagerId);
            if (manager is null) return BadRequest("Manager not found.");

            // Validate urgency
            if (dto.UrgencyLevel < 1 || dto.UrgencyLevel > 5)
                return BadRequest("UrgencyLevel must be between 1 and 5.");

            // Validate deadline
            if (dto.Deadline <= DateTime.UtcNow)
                return BadRequest("Deadline must be in the future.");

            // Apply escalation
            complaint.IsEscalated = true;
            complaint.EscalatedAt = DateTime.UtcNow;

            complaint.EscalatedById = user.Id;
            complaint.EscalatedToManagerId = dto.ManagerId;

            complaint.UrgencyLevel = dto.UrgencyLevel;
            complaint.EscalationDeadline = dto.Deadline;

            complaint.Priority = ComplaintPriority.High;

            // Mark management as notified
            complaint.ManagementNotified = true;
            complaint.ManagementNotifiedAt = DateTime.UtcNow;

            await _complaintRepo.CommitAsync();

            return Ok(new
            {
                complaint.Id,
                complaint.IsEscalated,
                escalatedAt = complaint.EscalatedAt,
                escalatedById = complaint.EscalatedById,
                managerId = complaint.EscalatedToManagerId,
                urgencyLevel = complaint.UrgencyLevel,
                deadline = complaint.EscalationDeadline,
                managementNotified = complaint.ManagementNotified,
                managementNotifiedAt = complaint.ManagementNotifiedAt
            });
        }

        #endregion
    }
}
