import { z } from 'zod';

export const customerSchema = z.object({
    id: z.number().optional(),
    name: z.string()
        .trim()
        .min(1, "Ad Soyad zorunludur")
        .max(100, "Ad Soyad en fazla 100 karakter olabilir"),

    email: z.string()
        .trim()
        .email("Geçerli bir e-posta adresi giriniz")
        .max(100, "E-posta en fazla 100 karakter olabilir")
        .or(z.literal(''))
        .transform(val => val === '' ? null : val)
        .nullable()
        .optional(),

    phone: z.string()
        .trim()
        .max(20, "Telefon numarası en fazla 20 karakter olabilir")
        .or(z.literal(''))
        .transform(val => val === '' ? null : val)
        .nullable()
        .optional(),

    instagramHandle: z.string()
        .trim()
        .max(100, "Instagram adı en fazla 100 karakter olabilir")
        .transform(val => val && val.startsWith('@') ? val.slice(1) : val)
        .or(z.literal(''))
        .nullable()
        .optional(),

    address: z.string()
        .trim()
        .max(1000, "Adres çok uzun (Max 1000 karakter)")
        .or(z.literal(''))
        .nullable()
        .optional()
});

export type Customer = z.infer<typeof customerSchema>;

export const defaultCustomerValues = {
    name: '',
    email: '',
    phone: '',
    instagramHandle: '',
    address: ''
};