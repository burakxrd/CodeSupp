<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import api from '../../services/api'; // api servisinizin yolu
import { useToast } from 'vue-toastification';

// --- TYPES ---
// Müşteri ID'si çıkarıldı
interface IncomeDTO {
    id: number;
    description: string;
    paymentType: string;
    amount: number | string;
    date: string;
}

// Düzenlenecek kayıt tipi
interface IncomeToEdit {
    id: number;
    description?: string;
    paymentType?: string;
    amount: number;
    date: string;
    [key: string]: any;
}

// --- PROPS ---
interface Props {
    isOpen: boolean;
    incomeToEdit?: IncomeToEdit | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    incomeToEdit: null
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save-success'): void;
}>();

const toast = useToast();
const isLoading = ref<boolean>(false);
const paymentTypes = ['Nakit', 'Kredi Kartı', 'Havale/EFT'];

// Varsayılan tarih (YYYY-MM-DD)
const getTodayStr = (): string => new Date().toISOString().split('T')[0];

// Form State (Müşteri ID yok)
const formData = ref<IncomeDTO>({
    id: 0,
    description: '',
    paymentType: 'Nakit',
    amount: '',
    date: getTodayStr()
});

// Formu Sıfırla
const resetForm = () => {
    formData.value = {
        id: 0,
        description: '',
        paymentType: 'Nakit',
        amount: '',
        date: getTodayStr()
    };
};

// Edit Modu Kontrolü
const isEditing = computed(() => !!props.incomeToEdit);

// Props veya Modal Durumu Değişince
watch(() => props.isOpen, (newVal) => {
    if (newVal) {
        // Edit modu ise formu doldur
        if (props.incomeToEdit) {
            formData.value = {
                id: props.incomeToEdit.id,
                description: props.incomeToEdit.description || '',
                paymentType: props.incomeToEdit.paymentType || 'Nakit',
                amount: props.incomeToEdit.amount,
                date: props.incomeToEdit.date ? new Date(props.incomeToEdit.date).toISOString().split('T')[0] : getTodayStr()
            };
        } else {
            resetForm();
        }
    }
});

const handleSubmit = async () => {
    try {
        isLoading.value = true;
        
        // Payload'da customerId yok
        const payload = {
            id: isEditing.value && props.incomeToEdit ? props.incomeToEdit.id : 0,
            description: formData.value.description,
            paymentType: formData.value.paymentType,
            amount: parseFloat(String(formData.value.amount)),
            date: formData.value.date,
            type: 'Income' // Backend'de ayrım yapılması gerekiyorsa diye ekledim (Opsiyonel)
        };

        if (isEditing.value && props.incomeToEdit) {
            // NOT: api.updateIncome metodunuz yoksa payment metodunu veya genel işlem metodunu kullanın
            await api.updatePayment(payload.id, payload); 
            toast.success("Gelir kaydı güncellendi.");
        } else {
            // NOT: Burada 'createPayment' yerine genel bir 'createIncome' veya customerId nullable olan bir endpoint kullanılmalı.
            // Şimdilik createPayment kullanıyorsak backend'in customerId olmadan kabul ettiğinden emin olun.
            // Veya: await api.createIncome(payload);
            await api.createPayment(payload); 
            toast.success("Gelir başarıyla eklendi.");
        }

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
                    {{ isEditing ? 'Gelir Kaydını Düzenle' : 'Yeni Gelir Ekle' }}
                </h3>
                <button @click="$emit('close')" class="text-gray-400 hover:text-white transition-colors">
                    ✕
                </button>
            </div>
            
            <form @submit.prevent="handleSubmit" class="space-y-5">
                
                <div class="grid grid-cols-2 gap-5">
                    <div>
                        <label class="block text-gray-400 text-sm mb-2 font-medium">Tutar (TL) <span class="text-red-500">*</span></label>
                        <input 
                            v-model="formData.amount" 
                            type="number" 
                            step="0.01" 
                            placeholder="0.00" 
                            class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#22c55e] transition-all font-mono" 
                            required
                        >
                        </div>
                    <div>
                        <label class="block text-gray-400 text-sm mb-2 font-medium">Ödeme Yöntemi</label>
                        <div class="relative">
                            <select 
                                v-model="formData.paymentType" 
                                class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#22c55e] appearance-none"
                            >
                                <option v-for="type in paymentTypes" :key="type" :value="type">{{ type }}</option>
                            </select>
                            <div class="absolute inset-y-0 right-0 flex items-center px-3 pointer-events-none text-gray-500">▼</div>
                        </div>
                    </div>
                </div>

                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Tarih</label>
                    <input 
                        v-model="formData.date" 
                        type="date" 
                        class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#22c55e] [color-scheme:dark]" 
                        required
                    >
                </div>

                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Açıklama (Opsiyonel)</label>
                    <input 
                        v-model="formData.description" 
                        placeholder="Örn: Hurda satışı, Faiz geliri..." 
                        class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#22c55e]" 
                    >
                </div>

                <div class="flex justify-end gap-4 pt-4 border-t border-gray-700">
                    <button type="button" @click="$emit('close')" class="text-gray-400 hover:text-white px-5 py-2 transition-colors font-medium">
                        İptal
                    </button>
                    <button 
                        type="submit" 
                        :disabled="isLoading"
                        class="bg-[#22c55e] hover:bg-[#16a34a] text-white px-8 py-3 rounded-xl font-bold shadow-lg shadow-green-500/20 transition-all transform active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        <span v-if="isLoading" class="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                        {{ isEditing ? 'Güncelle' : 'Kaydet' }}
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>