using AutoMapper;
using CodeSupp.Models;
using CodeSupp.Services.Finance;
using CodeSupp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSupp.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly IMapper _mapper; // AutoMapper eklendi

        public ExpensesController(IExpenseService expenseService, IMapper mapper)
        {
            _expenseService = expenseService;
            _mapper = mapper;
        }

        // GET: api/Expenses
        [HttpGet]
        public async Task<IActionResult> GetExpenses(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "date",
            [FromQuery] string? sortDir = "desc",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
        )
        {
            var result = await _expenseService.GetExpensesAsync(
                page, pageSize, search, sortBy, sortDir, startDate, endDate);

            return Ok(result);
        }

        // GET: api/Expenses/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpense(int id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null) return NotFound();

            return Ok(expense);
        }

        // POST: api/Expenses
        [HttpPost]
        public async Task<IActionResult> CreateExpense([FromBody] ExpenseViewModel model) // [DÜZELTME] ViewModel kullanıldı
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // ViewModel -> Entity dönüşümü (TenantId burada yok, DbContext otomatik atayacak)
            var expense = _mapper.Map<Expense>(model);

            var createdExpense = await _expenseService.CreateExpenseAsync(expense);

            return CreatedAtAction(nameof(GetExpense), new { id = createdExpense.Id }, createdExpense);
        }

        // PUT: api/Expenses/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] ExpenseViewModel model) // [DÜZELTME] ViewModel kullanıldı
        {
            if (model.Id.HasValue && id != model.Id.Value)
                return BadRequest(new { Message = "ID uyuşmazlığı." });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Önce veritabanındaki orijinal kaydı çek
            // (Bunu yapmazsak TenantId kaybolabilir veya güvenlik açığı oluşur)
            var existingExpense = await _expenseService.GetExpenseByIdAsync(id);

            if (existingExpense == null) return NotFound();

            // 2. ViewModel'deki değişiklikleri mevcut Entity'nin üzerine yaz
            // AutoMapper, sadece ViewModel'de olan alanları (Desc, Amount, Date) günceller.
            // TenantId ve diğer kritik alanlara DOKUNMAZ.
            _mapper.Map(model, existingExpense);

            // 3. Kaydet
            await _expenseService.UpdateExpenseAsync(existingExpense);

            return NoContent();
        }

        // DELETE: api/Expenses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var existingExpense = await _expenseService.GetExpenseByIdAsync(id);
            if (existingExpense == null) return NotFound();

            await _expenseService.DeleteExpenseAsync(id);

            return NoContent();
        }
    }
}