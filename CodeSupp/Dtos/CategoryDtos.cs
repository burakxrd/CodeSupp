using CodeSupp.Enums; 
using System.ComponentModel.DataAnnotations;

namespace CodeSupp.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ProductCount { get; set; }

        public CategoryType Type { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Kategori adı boş olamaz.")]
        [StringLength(100, ErrorMessage = "Çok uzun bir kategori adı girdiniz.")]
        public string Name { get; set; } = null!;

        public CategoryType Type { get; set; } = CategoryType.Standard;
    }

    public class UpdateCategoryDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı boş olamaz.")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public CategoryType Type { get; set; }
    }
}