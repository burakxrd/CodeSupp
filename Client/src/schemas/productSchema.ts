import { z } from 'zod';

const numericString = (schema: z.ZodTypeAny) => z.preprocess((val) => {
    if (val === '' || val === null) return undefined;
    const n = Number(val);
    return isNaN(n) ? val : n;
}, schema);

export const productSchema = z.object({
    id: z.number().optional(),

    name: z.string()
        .min(1, "Ürün adı zorunludur")
        .min(2, "Ürün adı çok kısa"),

    sku: z.string()
        .max(50, "SKU çok uzun")
        .optional()
        .or(z.literal('')),

    barcode: z.string().optional().or(z.literal('')),

    price: numericString(
        z.number({ invalid_type_error: "Geçerli bir fiyat giriniz" })
            .min(0.01, "Fiyat 0'dan büyük olmalıdır")
    ),

    stock: numericString(
        z.number({ invalid_type_error: "Stok miktarı sayı olmalıdır" })
            .min(0, "Stok negatif olamaz")
    ),

    categoryId: numericString(
        z.number({ invalid_type_error: "Lütfen bir kategori seçiniz" })
            .min(1, "Lütfen bir kategori seçiniz")
    ),

    description: z.string()
        .max(500, "Açıklama 500 karakteri geçemez")
        .optional()
        .or(z.literal('')),

    imageUrl: z.string().optional().or(z.literal(null))
});

export type Product = z.infer<typeof productSchema>;

export const defaultProductValues = {
    name: '',
    sku: '',
    barcode: '',
    price: '',
    stock: 0,
    categoryId: '',
    description: '',
    imageUrl: null
};