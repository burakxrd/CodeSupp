import { ref, computed } from 'vue';
import api from '../../services/api';
import type { Purchase } from '../../types'; 
import { useToast } from 'vue-toastification';

// --- TİP TANIMLAMALARI ---

// Form State (Input değerleri anlık olarak string olabileceği için union type)
export interface PurchaseFormState {
    id: number | null;
    productId: number | string;
    quantityInUnits: number | string;
    productPricePerUnit: number | string;
    totalKg: number | string;
    shippingCostPerKg: number | string;
    description: string;
    purchaseDate: string;
}

// Composable Props
interface UsePurchaseFormProps {
    purchaseToEdit?: Purchase | null;
}

// ✅ SENIOR DOKUNUŞU: Strict Emit Type
// Bu composable'ı kullanan component sadece bu eventleri bu imzalarla atabilir.
export type PurchaseFormEmit = {
    (e: 'close'): void;
    (e: 'save-success'): void;
};

// --- COMPOSABLE ---

export function usePurchaseForm(
    props: UsePurchaseFormProps, 
    emit: PurchaseFormEmit // ✅ Artık 'any' yok, katı kural var.
) {
    const toast = useToast();
    const isEditing = ref(false);
    const isLoading = ref(false);

    // Form Başlangıç Değerleri
    const form = ref<PurchaseFormState>({
        id: null,
        productId: '',
        quantityInUnits: 1,
        productPricePerUnit: 0,
        totalKg: 0,
        shippingCostPerKg: 0,
        description: '', 
        purchaseDate: new Date().toISOString().split('T')[0]
    });

    const searchQuery = ref('');
    
    // Maliyet Hesabı (Computed)
    const totalCost = computed(() => {
        const qty = parseFloat(String(form.value.quantityInUnits)) || 0;
        const price = parseFloat(String(form.value.productPricePerUnit)) || 0;
        const kg = parseFloat(String(form.value.totalKg)) || 0;
        const shipping = parseFloat(String(form.value.shippingCostPerKg)) || 0;
        
        return (qty * price) + (kg * shipping);
    });

    // Formu Başlat / Resetle
    const initForm = () => {
        if (props.purchaseToEdit) {
            isEditing.value = true;
            
            // Backend bazen standart dışı alan isimleri (legacy) gönderebilir.
            // Purchase tipini korurken, ekstra alanlara erişim izni vermek için Intersection Type kullanıyoruz.
            const item = props.purchaseToEdit as Purchase & Record<string, any>;
            
            form.value = {
                id: item.id,
                productId: item.productId,
                // Fallback mekanizması (quantityInUnits yoksa quantity'ye bak)
                quantityInUnits: item.quantityInUnits ?? item.quantity ?? 1,
                productPricePerUnit: item.productPricePerUnit ?? item.price ?? 0,
                totalKg: item.totalKg ?? 0,
                shippingCostPerKg: item.shippingCostPerKg ?? 0,
                description: item.description || '',
                purchaseDate: item.purchaseDate 
                    ? item.purchaseDate.split('T')[0] 
                    : new Date().toISOString().split('T')[0]
            };

            // Arama kutusunu doldur (Ürün adı ilişkiden gelir)
            const pName = item.product?.name || item.productName || '';
            searchQuery.value = pName;
        } else {
            // Yeni Kayıt Modu
            isEditing.value = false;
            form.value = {
                id: null,
                productId: '',
                quantityInUnits: 1,
                productPricePerUnit: 0,
                totalKg: 0,
                shippingCostPerKg: 0,
                description: '',
                purchaseDate: new Date().toISOString().split('T')[0]
            };
            searchQuery.value = '';
        }
    };

    // Form Gönder
    const submitForm = async () => {
        if (!form.value.productId) {
            toast.warning("Lütfen bir ürün seçiniz.");
            return;
        }
        
        isLoading.value = true;
        try {
            const rawData = { ...form.value };
            
            // Backend Payload Hazırlığı
            const payload = {
                productId: parseInt(String(rawData.productId)),
                quantityInUnits: parseInt(String(rawData.quantityInUnits)) || 0,
                productPricePerUnit: parseFloat(String(rawData.productPricePerUnit)) || 0,
                totalKg: parseFloat(String(rawData.totalKg)) || 0,
                shippingCostPerKg: parseFloat(String(rawData.shippingCostPerKg)) || 0,
                purchaseDate: rawData.purchaseDate,
                description: rawData.description?.trim() || '-',
                totalCost: totalCost.value 
            };

            if (isEditing.value && form.value.id) {
                await api.updatePurchase(form.value.id, payload);
                toast.success("Satın alma kaydı güncellendi.");
            } else {
                await api.createPurchase(payload);
                toast.success("Yeni satın alma kaydı oluşturuldu.");
            }
            
            // ✅ Autocomplete destekli type-safe emitler
            emit('save-success');
            emit('close');
            
        } catch (err: any) {
            console.error(err);
            const msg = err.response?.data?.message || err.message || "İşlem başarısız.";
            toast.error(msg);
        } finally {
            isLoading.value = false;
        }
    };

    return {
        form,
        isEditing,
        isLoading,
        searchQuery,
        totalCost,
        initForm,
        submitForm
    };
}