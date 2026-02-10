import { ref, type Ref } from 'vue';
import { useToast } from 'vue-toastification';
import { customerSchema, defaultCustomerValues, type Customer } from '../../schemas/customerSchema';
import customerService from '../../services/customerService';

export function useCustomerForm() {
    const toast = useToast();

    // --- STATE ---
    const isLoading = ref(false);
    const errors: Ref<Record<string, string>> = ref({});

    // Form verisi
    const formData: Ref<Customer> = ref({ ...defaultCustomerValues });

    // --- ACTIONS ---

    // 1. Formu Doldur (Edit Modu veya Reset)
    const setForm = (data: Customer | null = null) => {
        errors.value = {};
        if (data) {
            // Edit Modu
            formData.value = { ...data };
        } else {
            // Yeni Kayıt Modu
            formData.value = { ...defaultCustomerValues };
        }
    };

    // 2. Validasyon (Zod ile)
    const validate = (): boolean => {
        const result = customerSchema.safeParse(formData.value);

        if (!result.success) {
            const fieldErrors = result.error.flatten().fieldErrors;
            errors.value = Object.keys(fieldErrors).reduce<Record<string, string>>((acc, key) => {
                const fieldKey = key as keyof typeof fieldErrors;
                const errorMsg = fieldErrors[fieldKey]?.[0];
                
                if (errorMsg) {
                    acc[key] = errorMsg;
                }
                return acc;
            }, {});
            return false;
        }
        errors.value = {};
        return true;
    };

    // 3. Gönder (Submit)
    const handleSubmit = async (onSuccess?: () => void) => {
        if (!validate()) {
            toast.warning("Lütfen formdaki hataları kontrol edin.");
            return;
        }

        isLoading.value = true;

        try {
            const data = { ...formData.value };

            // [GÜNCELLENDİ] customerService kullanımı
            if (data.id) {
                await customerService.updateCustomer(data.id, data);
                toast.success("Müşteri başarıyla güncellendi.");
            } else {
                await customerService.createCustomer(data);
                toast.success("Müşteri başarıyla oluşturuldu.");
            }

            if (onSuccess) onSuccess();

        } catch (err: any) {
            console.error("Form işlem hatası:", err);
            // Servisten gelen hatayı yakala
            const msg = err.response?.data?.message || err.customMessage || "Bir hata oluştu.";
            toast.error(msg);
        } finally {
            isLoading.value = false;
        }
    };

    return {
        formData,
        errors,
        isLoading,
        setForm,
        handleSubmit,
        validate
    };
}