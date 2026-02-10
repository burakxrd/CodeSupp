using CodeSupp.Data;
using CodeSupp.Models;
using CodeSupp.Services.Customers;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomerService _customerService;

        // ITenantService kaldırıldı (Gereksiz)
        public CustomersController(ApplicationDbContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        // --- Müşteri Listesi ---
        [HttpGet]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            // [DÜZELTME] Sayfalama parametrelerini servise gönderiyoruz.
            // Servis zaten PaginatedResult dönüyor, burada tekrar Skip/Take yapmamıza gerek yok.
            // Bu yöntem performans açısından çok daha sağlıklıdır (DB-Side Pagination).

            var result = await _customerService.GetCustomersAsync(page, pageSize, status, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            // Global Filter Garantisi
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null) return NotFound();

            // Müşterinin son 50 siparişini getir
            // Global Filter 'Sales' tablosunda da var, ekstra kontrole gerek yok.
            var sales = await _context.Sales
                .AsNoTracking()
                .Where(s => s.CustomerId == id)
                .Include(s => s.SaleItems)
                .OrderByDescending(s => s.SaleDate)
                .Take(50)
                .ToListAsync();

            var viewModel = new CustomerProfileViewModel
            {
                Customer = customer,
                Sales = sales
            };
            return Ok(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var customer = new Customer
            {
                Name = model.Name ?? "İsimsiz Müşteri",
                Email = model.Email,
                Phone = model.Phone,
                InstagramHandle = model.InstagramHandle,
                Address = model.Address,
                // TenantId DbContext tarafından otomatik atanıyor.
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerCreateUpdateViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Global Filter Garantisi
            var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customerInDb == null) return NotFound();

            customerInDb.Name = model.Name ?? customerInDb.Name;
            customerInDb.Email = model.Email;
            customerInDb.Phone = model.Phone;
            customerInDb.InstagramHandle = model.InstagramHandle;
            customerInDb.Address = model.Address;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            // Global Filter Garantisi
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null) return NotFound();

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { Message = "Bu müşterinin ilişkili kayıtları (sipariş/ödeme) var, silinemez." });
            }
            return NoContent();
        }
    }
}