<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import api from '../../services/api';
import { useToast } from 'vue-toastification';
import type { Customer } from '../../types'; 

// --- TYPES ---
interface PaymentDTO {
    id: number;
    customerId: number | string;
    description: string;
    paymentType: number; // ✅ DÜZELTME: string → number
    amount: number | string;
    date: string;
}

interface PaymentToEdit {
    id: number;
    customerId?: number | string;
    description?: string;
    paymentType?: number; // ✅ DÜZELTME: string → number
    amount: number;
    date: string;
    [key: string]: any;
}

// --- PROPS ---
interface Props {
    isOpen: boolean;
    paymentToEdit?: PaymentToEdit | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    paymentToEdit: null
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save-success'): void;
}>();

const toast = useToast();
const isLoading = ref<boolean>(false);
const customers = ref<Customer[]>([]); 

// ✅ DÜZELTME: Enum mapping
const paymentTypes = [
    { label: 'Nakit', value: 3 },
    { label: 'Kredi Kartı', value: 1 },
    { label: 'Havale/EFT', value: 2 }
];

const getTodayStr = (): string => new Date().toISOString().split('T')[0];

// Form State
const formData = ref<PaymentDTO>({
    id: 0,
    customerId: '',
    description: '',
    paymentType: 3, // ✅ DÜZELTME: 'Nakit' → 3 (Cash)
    amount: '',
    date: getTodayStr()
});

const fetchCustomers = async () => {
    try {
        const res: any = await api.getCustomers(1, 1000); 
        customers.value = res.items || [];
    } catch (err) {
        console.error("Müşteriler yüklenemedi", err);
        toast.error("Müşteri listesi yüklenemedi.");
    }
};

const resetForm = () => {
    formData.value = {
        id: 0,
        customerId: '',
        description: '',
        paymentType: 3, // ✅ DÜZELTME
        amount: '',
        date: getTodayStr()
    };
};

const isEditing = computed(() => !!props.paymentToEdit);

watch(() => props.isOpen, (newVal) => {
    if (newVal) {
        if (customers.value.length === 0) fetchCustomers();

        if (props.paymentToEdit) {
            formData.value = {
                id: props.paymentToEdit.id,
                customerId: props.paymentToEdit.customerId || '',
                description: props.paymentToEdit.description || '',
                paymentType: props.paymentToEdit.paymentType || 3, // ✅ DÜZELTME
                amount: props.paymentToEdit.amount,
                date: props.paymentToEdit.date ? new Date(props.paymentToEdit.date).toISOString().split('T')[0] : getTodayStr()
            };
        } else {
            resetForm();
        }
    }
});

const handleSubmit = async () => {
    try {
        isLoading.value = true;
        
        const payload = {
            id: isEditing.value && props.paymentToEdit ? props.paymentToEdit.id : 0,
            customerId: Number(formData.value.customerId),
            description: formData.value.description,
            paymentType: Number(formData.value.paymentType), // ✅ DÜZELTME: Sayısal değer gönder
            amount: parseFloat(String(formData.value.amount)),
            date: formData.value.date
        };

        if (isEditing.value && props.paymentToEdit) {
            await api.updatePayment(payload.id, payload);
            toast.success("Tahsilat başarıyla güncellendi.");
        } else {
            await api.createPayment(payload);
            toast.success("Tahsilat başarıyla kaydedildi.");
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
                    {{ isEditing ? 'Tahsilatı Düzenle' : 'Yeni Tahsilat Al' }}
                </h3>
                <button @click="$emit('close')" class="text-gray-400 hover:text-white transition-colors">
                    ✕
                </button>
            </div>
            
            <form @submit.prevent="handleSubmit" class="space-y-5">
                
                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Müşteri <span class="text-red-500">*</span></label>
                    <div class="relative">
                        <select 
                            v-model="formData.customerId" 
                            required
                            class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#3699FF] appearance-none"
                        >
                            <option value="" disabled>Müşteri Seçiniz</option>
                            <option v-for="c in customers" :key="c.id" :value="c.id">{{ c.name }}</option>
                        </select>
                        <div class="absolute inset-y-0 right-0 flex items-center px-3 pointer-events-none text-gray-500">▼</div>
                    </div>
                </div>

                <div class="grid grid-cols-2 gap-5">
                    <div>
                        <label class="block text-gray-400 text-sm mb-2 font-medium">Tutar (TL) <span class="text-red-500">*</span></label>
                        <input 
                            v-model="formData.amount" 
                            type="number" 
                            step="0.01" 
                            placeholder="0.00" 
                            class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#3699FF] transition-all font-mono" 
                            required
                        >
                    </div>
                    <div>
                        <label class="block text-gray-400 text-sm mb-2 font-medium">Ödeme Yöntemi</label>
                        <div class="relative">
                            <!-- ✅ DÜZELTME: value binding number olarak -->
                            <select 
                                v-model.number="formData.paymentType" 
                                class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#3699FF] appearance-none"
                            >
                                <option v-for="type in paymentTypes" :key="type.value" :value="type.value">
                                    {{ type.label }}
                                </option>
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
                        class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#3699FF] [color-scheme:dark]" 
                        required
                    >
                </div>

                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Açıklama (Opsiyonel)</label>
                    <input 
                        v-model="formData.description" 
                        placeholder="Örn: Ocak ayı taksiti" 
                        class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#3699FF]" 
                    >
                </div>

                <div class="flex justify-end gap-4 pt-4 border-t border-gray-700">
                    <button type="button" @click="$emit('close')" class="text-gray-400 hover:text-white px-5 py-2 transition-colors font-medium">
                        İptal
                    </button>
                    <button 
                        type="submit" 
                        :disabled="isLoading"
                        class="bg-[#3699FF] hover:bg-[#0073E9] text-white px-8 py-3 rounded-xl font-bold shadow-lg shadow-blue-500/20 transition-all transform active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                    >
                        <span v-if="isLoading" class="animate-spin h-4 w-4 border-2 border-white border-t-transparent rounded-full"></span>
                        {{ isEditing ? 'Güncelle' : 'Kaydet' }}
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>