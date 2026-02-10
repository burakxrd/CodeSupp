<script setup lang="ts">
import { ref, watch } from 'vue';
import api from '../../services/api';

// --- TYPES ---
interface Sale {
    id: number;
    shippingStatus?: string;      // [DÜZELTME] Opsiyonel (?)
    rowVersion?: string;
    customerId?: number | null;   // [DÜZELTME] Opsiyonel (?)
    remainingAmount?: number;     // [DÜZELTME] Opsiyonel (?)
    [key: string]: any;
}

interface PaymentFormState {
    orderId: number | null;
    customerId: number | null;
    amount: number | string; 
    remaining: number;
    totalDebt: number;
    date: string;
    description: string;
    rowVersion: string | null | undefined;
}

// --- PROPS & EMITS ---
interface Props {
    isOpen: boolean;
    sale?: Sale | null; // null olabilir
}

const props = defineProps<Props>();

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'payment-success'): void;
}>();

// --- LOCAL STATE ---
const paymentForm = ref<PaymentFormState>({
    orderId: null,
    customerId: null,
    amount: 0,
    remaining: 0, 
    totalDebt: 0, 
    date: new Date().toISOString().substring(0, 10),
    description: '',
    rowVersion: null 
});

// --- HELPER FUNCTIONS ---
const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
};

// --- LOGIC ---
watch(() => props.sale, (newSale) => {
    if (newSale) {
        const remaining = Number(newSale.remainingAmount) || 0;
        paymentForm.value = {
            orderId: newSale.id,
            customerId: newSale.customerId || null,
            amount: remaining, 
            remaining: remaining, 
            totalDebt: remaining, 
            date: new Date().toISOString().substring(0, 10),
            description: `Sipariş #${newSale.id} ödemesi`,
            rowVersion: newSale.rowVersion 
        };
    }
}, { immediate: true });

const setAmountFraction = (denominator: number) => {
    if (paymentForm.value.totalDebt > 0) {
        const val = paymentForm.value.totalDebt / denominator;
        paymentForm.value.amount = Math.round(val * 100) / 100;
    }
};

const submitPayment = async () => {
    try {
        const payload = {
            customerId: paymentForm.value.customerId,
            saleId: paymentForm.value.orderId, 
            amount: typeof paymentForm.value.amount === 'string' ? parseFloat(paymentForm.value.amount) : paymentForm.value.amount,
            date: paymentForm.value.date,
            description: paymentForm.value.description,
            rowVersion: paymentForm.value.rowVersion 
        };

        await api.createPayment(payload);
        
        alert("Tahsilat başarıyla işlendi! 💳");
        emit('payment-success'); 
        emit('close');

    } catch (err: any) {
        console.error("Tahsilat Hatası:", err);
        let msg = "İşlem başarısız!";
        
        if (err.response?.status === 409) {
            msg = "Veri değişmiş! Lütfen sayfayı yenileyip tekrar deneyin.";
        } else if (err.response?.data?.message) {
            msg = err.response.data.message;
        }
        alert("🛑 " + msg);
    }
};
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-[999] flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
        <div class="bg-[#1E1E2D] w-full max-w-md rounded-xl shadow-2xl border border-gray-700 p-6 relative">
            <h3 class="text-xl font-bold text-white mb-2 text-center">Tahsilat Yap</h3>
            <p class="text-gray-400 text-center text-sm mb-6">Sipariş #{{ paymentForm.orderId }}</p>
            
            <form @submit.prevent="submitPayment" class="space-y-6">
                <div class="bg-gray-800 p-4 rounded-xl text-center border border-gray-700">
                    <span class="block text-gray-500 text-xs mb-1 uppercase tracking-wide">Kalan Bakiye</span>
                    <span class="text-red-400 text-2xl font-bold font-mono">{{ formatCurrency(paymentForm.remaining) }}</span>
                </div>

                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Ödenen Tutar (TL)</label>
                    <input v-model="paymentForm.amount" type="number" step="0.01" class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white text-3xl font-bold outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-center placeholder-gray-700 mb-2" placeholder="0.00" autoFocus>
                    
                    <div class="relative w-full h-6 flex items-center">
                        <input type="range" min="0" :max="paymentForm.totalDebt" step="10" v-model.number="paymentForm.amount" class="w-full h-2 bg-gray-700 rounded-lg appearance-none cursor-pointer accent-indigo-500">
                    </div>
                    <div class="flex justify-between gap-2 mt-3">
                        <button type="button" @click="setAmountFraction(2)" class="flex-1 bg-gray-700 hover:bg-gray-600 text-gray-300 text-xs py-1.5 rounded border border-gray-600">1/2</button>
                        <button type="button" @click="setAmountFraction(3)" class="flex-1 bg-gray-700 hover:bg-gray-600 text-gray-300 text-xs py-1.5 rounded border border-gray-600">1/3</button>
                        <button type="button" @click="setAmountFraction(4)" class="flex-1 bg-gray-700 hover:bg-gray-600 text-gray-300 text-xs py-1.5 rounded border border-gray-600">1/4</button>
                        <button type="button" @click="setAmountFraction(5)" class="flex-1 bg-gray-700 hover:bg-gray-600 text-gray-300 text-xs py-1.5 rounded border border-gray-600">1/5</button>
                    </div>
                </div>

                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Ödeme Tarihi</label>
                    <input v-model="paymentForm.date" type="date" class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-indigo-500 transition-all text-base [color-scheme:dark]">
                </div>

                <div>
                    <label class="block text-gray-400 text-sm mb-2 font-medium">Açıklama</label>
                    <input v-model="paymentForm.description" type="text" class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-indigo-500 transition-all text-base">
                </div>

                <div class="flex justify-end gap-3 pt-2">
                    <button type="button" @click="emit('close')" class="text-gray-400 hover:text-white px-5 py-3 transition-colors font-medium">İptal</button>
                    <button type="submit" class="bg-indigo-600 hover:bg-indigo-500 text-white px-8 py-3 rounded-xl font-bold shadow-lg shadow-indigo-500/20 transition-all transform active:scale-95 w-full sm:w-auto">
                        Ödemeyi Onayla
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>