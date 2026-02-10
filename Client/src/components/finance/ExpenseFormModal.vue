<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import api from '../../services/api';
import { useToast } from 'vue-toastification';
import type { Expense } from '../../types'; // Global tipler

// --- TYPES ---

// Formun yerel state'i (Inputlar string dönebilir, o yüzden union type kullanıyoruz)
interface ExpenseDTO {
    description: string;
    amount: number | string;
    date: string;
}

// Props
interface Props {
    isOpen: boolean;
    // Expense tipi veya null olabilir
    expenseToEdit?: Expense | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    expenseToEdit: null
});

// Emits
const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save-success'): void;
}>();

// --- STATE ---
const toast = useToast();
const isLoading = ref<boolean>(false);

// Varsayılan tarih (YYYY-MM-DD)
const getTodayStr = (): string => new Date().toISOString().split('T')[0];

const formData = ref<ExpenseDTO>({
    description: '',
    amount: '',
    date: getTodayStr()
});

// --- WATCHERS ---
// Modal açıldığında veya expenseToEdit değiştiğinde formu doldur
watch(
    () => props.expenseToEdit, 
    (newVal) => {
        if (newVal) {
            // Edit Modu
            formData.value = {
                description: newVal.description,
                amount: newVal.amount,
                // ISO string kontrolü
                date: newVal.date ? new Date(newVal.date).toISOString().split('T')[0] : getTodayStr()
            };
        } else {
            // Yeni Ekle Modu - Formu sıfırla
            formData.value = {
                description: '',
                amount: '',
                date: getTodayStr()
            };
        }
    }, 
    { immediate: true }
);

const isEditing = computed(() => !!props.expenseToEdit);

// --- ACTIONS ---
const handleSubmit = async () => {
    try {
        isLoading.value = true;
        
        // Payload hazırlığı: Type casting ve dönüşümler
        const payload = {
            id: isEditing.value && props.expenseToEdit ? props.expenseToEdit.id : 0,
            description: formData.value.description,
            amount: parseFloat(String(formData.value.amount)), // String gelirse floata çevir
            date: formData.value.date
        };

        if (isEditing.value) {
            await api.updateExpense(payload.id, payload);
            toast.success("Gider başarıyla güncellendi.");
        } else {
            await api.createExpense(payload);
            toast.success("Gider başarıyla kaydedildi.");
        }

        // Başarılı olursa parent'a haber ver
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
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-[999] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
        <div class="bg-[#1E1E2D] w-full max-w-lg rounded-xl shadow-2xl border border-gray-700 p-8 relative">
            
            <div class="flex justify-between items-center mb-6 border-b border-gray-700 pb-4">
                <h3 class="text-2xl font-bold text-white">
                    {{ isEditing ? 'Gideri Düzenle' : 'Yeni Gider Ekle' }}
                </h3>
                <button @click="$emit('close')" class="text-gray-400 hover:text-white transition-colors">
                    ✕
                </button>
            </div>
            
            <form @submit.prevent="handleSubmit" class="space-y-6">
                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Açıklama <span class="text-red-500">*</span></label>
                    <input 
                        v-model="formData.description" 
                        placeholder="Örn: Ofis Kirası" 
                        class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#F64E60] focus:ring-1 focus:ring-[#F64E60] transition-all text-base" 
                        required
                    >
                </div>

                <div class="grid grid-cols-2 gap-6">
                    <div>
                        <label class="block text-gray-400 text-sm mb-2 font-medium">Tutar (TL) <span class="text-red-500">*</span></label>
                        <input 
                            v-model="formData.amount" 
                            type="number" 
                            step="0.01" 
                            placeholder="0.00" 
                            class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#F64E60] focus:ring-1 focus:ring-[#F64E60] transition-all text-base" 
                            required
                        >
                    </div>
                    <div>
                        <label class="block text-gray-400 text-sm mb-2 font-medium">Tarih</label>
                        <input 
                            v-model="formData.date" 
                            type="date" 
                            class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#F64E60] focus:ring-1 focus:ring-[#F64E60] transition-all text-base [color-scheme:dark]" 
                            required
                        >
                    </div>
                </div>

                <div class="flex justify-end gap-4 pt-4 border-t border-gray-700">
                    <button type="button" @click="$emit('close')" class="text-gray-400 hover:text-white px-5 py-2 transition-colors font-medium">
                        İptal
                    </button>
                    <button 
                        type="submit" 
                        :disabled="isLoading"
                        class="bg-[#F64E60] hover:bg-[#D63D50] text-white px-8 py-3 rounded-xl font-bold shadow-lg shadow-red-500/20 transition-all transform active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        <span v-if="isLoading" class="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                        {{ isEditing ? 'Güncelle' : 'Kaydet' }}
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>