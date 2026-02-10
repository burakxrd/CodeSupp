using System.ComponentModel.DataAnnotations;

namespace CodeSupp.Dtos
{
    public class StockAdjustmentDto
    {
        [Required(ErrorMessage = "Düzeltme miktarı zorunludur.")]
        public int AdjustmentAmount { get; set; }

        [Required(ErrorMessage = "Lütfen bir açıklama/sebep giriniz.")]
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
        public string Reason { get; set; } = null!;
    }
}