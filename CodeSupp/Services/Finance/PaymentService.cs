using AutoMapper;
using CodeSupp.Data;
using CodeSupp.Enums;
using CodeSupp.Models;
using CodeSupp.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Services.Finance
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PaymentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<PaymentListViewModel>> GetPaymentsAsync(
            int page,
            int pageSize,
            string? search,
            string? sortBy,
            string? sortDir,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _context.Payments
                .AsNoTracking()
                .AsQueryable();

            // 1. Arama (ToUpper ile case-insensitive)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchUpper = search.Trim().ToUpperInvariant();

                query = query.Where(p =>
                    (p.Customer != null && p.Customer.Name.ToUpper().Contains(searchUpper)) ||
                    (p.Description != null && p.Description.ToUpper().Contains(searchUpper))
                );
            }

            // 2. Tarih
            if (startDate.HasValue) query = query.Where(p => p.Date >= startDate.Value);
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.Date <= end);
            }

            // 3. Sıralama
            bool isAsc = sortDir == "asc";
            query = sortBy switch
            {
                "amount" => isAsc ? query.OrderBy(p => p.Amount) : query.OrderByDescending(p => p.Amount),
                "date" => isAsc ? query.OrderBy(p => p.Date) : query.OrderByDescending(p => p.Date),
                _ => query.OrderByDescending(p => p.Date)
            };

            var totalCount = await query.CountAsync();

            // 4. Veri Çekme
            var items = await query
                .Include(p => p.Customer)
                .Include(p => p.Sale)
                    .ThenInclude(s => s!.SaleItems)
                        .ThenInclude(si => si.Product)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = _mapper.Map<List<PaymentListViewModel>>(items);

            return new PaginatedResult<PaymentListViewModel>(model, totalCount, page, pageSize);
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment> CreatePaymentAsync(PaymentViewModel model)
        {
            if (model.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
                if (!customerExists) throw new Exception("Seçilen müşteri bulunamadı.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = _mapper.Map<Payment>(model);
                payment.CreatedAt = DateTime.UtcNow;
                if (payment.Date.Date == DateTime.Today)
                {
                    payment.Date = DateTime.Now;
                }
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                var financeRecord = new Transaction
                {
                    Type = TransactionType.Income,
                    Category = payment.Category,
                    PaymentMethod = payment.PaymentMethod,
                    Amount = payment.Amount,
                    Description = payment.Description,
                    Date = payment.Date,
                    TenantId = payment.TenantId,
                    PaymentId = payment.Id
                };

                _context.Transactions.Add(financeRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return payment;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdatePaymentAsync(int id, PaymentViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
                if (payment == null) throw new Exception("Tahsilat bulunamadı.");
                var oldDate = payment.Date;

                _mapper.Map(model, payment);

                if (payment.Date.Date == oldDate.Date)
                {
                    payment.Date = oldDate;
                }
                else if (payment.Date.Date == DateTime.Today)
                {
                    payment.Date = DateTime.Now;
                }
                var financeRecord = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.PaymentId == id);

                if (financeRecord != null)
                {
                    financeRecord.Amount = payment.Amount;
                    financeRecord.Description = payment.Description;
                    financeRecord.Date = payment.Date;
                    financeRecord.Category = payment.Category;
                    financeRecord.PaymentMethod = payment.PaymentMethod;
                    _context.Transactions.Update(financeRecord);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeletePaymentAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);

                if (payment != null)
                {
                    var financeRecord = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.PaymentId == id);

                    if (financeRecord != null)
                    {
                        _context.Transactions.Remove(financeRecord);
                    }

                    _context.Payments.Remove(payment);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}