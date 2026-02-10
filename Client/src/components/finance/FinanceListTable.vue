<script setup lang="ts">
import { ref, onMounted, toRef, defineAsyncComponent } from 'vue';
import api from '../../services/api';
import { useDataTable } from '../../composables/useDataTable';
import { formatCurrency, formatDateTime } from '../../utils/formatters';
import type { Payment } from '../../types';

// --- TYPES ---
interface PaymentRow extends Payment {
    customerName?: string;
    productNames?: string[];
    paymentType?: number; // ✅ DÜZELTME: string → number
}

interface FilterProps {
    startDate: string | null;
    endDate: string | null;
    sortBy: string;
    sortDir: string;
}

// --- PROPS ---
const props = defineProps<{
    filters: FilterProps
}>();

// --- MODALS (Lazy Load) ---
const PaymentFormModal = defineAsyncComponent(() => 
  import('./FinanceFormModal.vue')
);

const IncomeModal = defineAsyncComponent(() => 
  import('../finance/IncomeModal.vue') 
);

// ✅ DÜZELTME: Enum mapping helper
const getPaymentTypeLabel = (value: number | undefined): string => {
    const map: Record<number, string> = {
        1: 'Kredi Kartı',
        2: 'Havale/EFT',
        3: 'Nakit',
        4: 'Diğer'
    };
    return map[value || 4] || 'Diğer';
};

const getPaymentTypeClass = (value: number | undefined): string => {
    if (value === 3) return 'bg-green-500/10 text-green-400 border border-green-500/20'; // Nakit
    if (value === 1) return 'bg-purple-500/10 text-purple-400 border border-purple-500/20'; // Kredi Kartı
    return 'bg-blue-500/10 text-blue-400 border border-blue-500/20'; // Diğer
};

// --- ADAPTER ---
const fetchPaymentsAdapter = async (p: any) => {
    
    const res: any = await api.getPayments(
        p.page,
        p.pageSize,
        p.search,
        props.filters.sortBy,         
        props.filters.sortDir,        
        props.filters.startDate || undefined, 
        props.filters.endDate || undefined    
    );

    return {
        items: res.items,
        totalCount: res.total || res.totalCount || 0,
        pageNumber: res.page || res.pageNumber || 1,
        totalPages: res.totalPages || 1,
        pageSize: p.pageSize
    };
};

// --- COMPOSABLE KULLANIMI ---
const {
    items: payments,
    isLoading,
    searchQuery,
    currentPage,
    totalPages,
    totalCount,
    pageSize,
    fetchData: fetchPayments, 
    changePage,
    handleDelete
} = useDataTable<PaymentRow>({
    apiFetch: fetchPaymentsAdapter,   
    apiDelete: (id) => api.deletePayment(Number(id)), 
    filters: toRef(props, 'filters'), 
    pageSizeInit: 10
});

// Manuel Search
let searchTimeout: ReturnType<typeof setTimeout>;
const handleSearch = (e: Event) => {
    const val = (e.target as HTMLInputElement).value;
    searchQuery.value = val;
    
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        fetchPayments();
    }, 300);
};

// --- MODAL STATES ---
const showModal = ref<boolean>(false);
const showIncomeModal = ref<boolean>(false); 
const selectedPayment = ref<PaymentRow | null>(null);

const openAddModal = () => {
    selectedPayment.value = null;
    showModal.value = true;
};

const openIncomeModal = () => {
    showIncomeModal.value = true;
};

const openEditModal = (payment: PaymentRow) => {
    selectedPayment.value = { ...payment };
    showModal.value = true;
};

const handleSaveSuccess = () => {
    fetchPayments(currentPage.value);
};

onMounted(() => {
    fetchPayments();
});
</script>

<template>
    <div>
        <div class="flex flex-col md:flex-row gap-4 justify-between items-center mb-6">
            <div class="relative w-full md:w-1/3">
                <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500">🔍</span>
                <input 
                    :value="searchQuery" 
                    @input="handleSearch"
                    type="text" 
                    placeholder="Müşteri veya açıklama ara..." 
                    class="w-full bg-[#151521] text-gray-300 border border-gray-700 rounded-xl py-2.5 pl-10 pr-4 focus:border-[#3699FF] outline-none transition-all text-sm"
                >
            </div>

            <div class="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
                <button 
                    @click="openIncomeModal" 
                    class="w-full sm:w-auto bg-[#22c55e] hover:bg-[#16a34a] text-white px-5 py-2.5 rounded-xl font-bold text-sm shadow-lg shadow-green-500/20 flex items-center justify-center gap-2 transition-all transform hover:scale-105 active:scale-95"
                >
                    <span>💰</span> Yeni Gelir Ekle
                </button>

                <button 
                    @click="openAddModal" 
                    class="w-full sm:w-auto bg-[#3699FF] hover:bg-[#0073E9] text-white px-5 py-2.5 rounded-xl font-bold text-sm shadow-lg shadow-blue-500/20 flex items-center justify-center gap-2 transition-all transform hover:scale-105 active:scale-95"
                >
                    <span>+</span> Yeni Tahsilat
                </button>
            </div>
        </div>

        <div class="bg-[#1E1E2D] rounded-xl shadow-xl border border-gray-800/50 flex flex-col min-h-[400px]">
            
            <div v-if="isLoading" class="p-10 text-center text-gray-500 flex flex-col items-center gap-2">
                <svg class="animate-spin h-8 w-8 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
                <span>Yükleniyor...</span>
            </div>
            
            <div v-else class="flex-1 flex flex-col">
                <div class="overflow-x-visible">
                    <table class="w-full text-left border-collapse min-w-[800px]">
                        <thead>
                            <tr class="bg-[#151521] text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-800">
                                <th class="p-5 pl-8 w-32">Tarih</th>
                                <th class="p-5 w-1/4">Müşteri</th>
                                <th class="p-5 w-1/4">Ürünler</th>
                                <th class="p-5">Ödeme Tipi</th>
                                <th class="p-5 text-right">Tutar</th>
                                <th class="p-5 text-right pr-8">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-800">
                            <tr v-for="payment in payments" :key="payment.id" class="group hover:bg-[#2B2B40] transition-colors relative">
                                
                                <td class="p-5 pl-8 text-gray-400 font-medium text-sm align-top pt-6">
                                    {{ formatDateTime(payment.date) }}
                                </td>
                                
                                <td class="p-5 align-top">
                                    <div class="flex items-start">
                                        <div class="h-9 w-9 rounded bg-gray-700 flex items-center justify-center text-sm font-bold text-white mr-3 uppercase flex-shrink-0 mt-0.5">
                                            {{ payment.customerName ? payment.customerName.charAt(0) : '💰' }}
                                        </div>
                                        <div>
                                            <div class="text-white font-medium text-base leading-tight">
                                                {{ payment.customerName ? '👤 ' + payment.customerName : '💰 Genel Gelir' }}
                                            </div>
                                            <div v-if="payment.description" class="text-xs text-gray-500 mt-1.5 line-clamp-2">{{ payment.description }}</div>
                                        </div>
                                    </div>
                                </td>   

                                <td class="p-5 align-top pt-6">
                                    <span v-if="!payment.productNames || payment.productNames.length === 0" class="text-gray-600 text-sm">-</span>
                                    
                                    <span v-else-if="payment.productNames.length === 1" class="text-gray-300 font-medium text-sm flex items-center gap-2">
                                        <span class="text-indigo-400">📦</span> {{ payment.productNames[0] }}
                                    </span>
                                    
                                    <div v-else class="relative group inline-block z-20">
                                        <button class="bg-[#151521] hover:bg-[#32324A] text-indigo-400 text-xs font-bold px-3 py-1.5 rounded-lg border border-indigo-500/30 transition-all flex items-center gap-2 shadow-sm group-hover:shadow-indigo-500/20">
                                            <span>📦</span> {{ payment.productNames.length }} Ürün
                                            <span class="text-[10px] ml-0.5 text-gray-500 group-hover:text-white transition-colors">▼</span>
                                        </button>
                                        <div class="absolute left-0 top-full mt-2 w-56 bg-[#1E1E2D] border border-gray-700 rounded-xl shadow-2xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 transform origin-top-left z-[9999]">
                                            <div class="bg-[#151521] px-4 py-2 text-[10px] text-gray-500 uppercase font-bold tracking-wider rounded-t-xl border-b border-gray-800">
                                                Satılan Ürünler
                                            </div>
                                            <ul class="py-2 max-h-48 overflow-y-auto custom-scrollbar">
                                                <li v-for="(name, index) in payment.productNames" :key="index" class="px-4 py-2 text-sm text-gray-300 hover:bg-white/5 border-b border-gray-800/50 last:border-0 flex items-center gap-2">
                                                    <span class="w-1.5 h-1.5 rounded-full bg-indigo-500"></span> {{ name }}
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </td>

                                <!-- ✅ DÜZELTME: Number → String dönüşümü -->
                                <td class="p-5 align-top pt-6">
                                    <span :class="['px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wide', getPaymentTypeClass(payment.paymentType)]">
                                        {{ getPaymentTypeLabel(payment.paymentType) }}
                                    </span>
                                </td>

                                <td class="p-5 text-right font-mono text-lg font-bold text-emerald-400 align-top pt-5">
                                    +{{ formatCurrency(payment.amount) }}
                                </td>

                                <td class="p-5 text-right pr-8 align-top pt-5">
                                    <div class="flex items-center justify-end gap-2">
                                        <button @click="openEditModal(payment)" class="p-2 rounded-lg bg-blue-500/10 text-blue-400 hover:bg-blue-500 hover:text-white transition-all" title="Düzenle">✏️</button>
                                        <button @click="handleDelete(payment.id)" class="p-2 rounded-lg bg-red-500/10 text-red-400 hover:bg-red-500 hover:text-white transition-all" title="Sil">🗑️</button>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div v-if="totalPages > 1" class="p-4 border-t border-gray-800 flex items-center justify-between bg-[#151521] mt-auto">
                    <span class="text-xs text-gray-500">Toplam {{ totalCount }} kayıttan {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} arası</span>
                    <div class="flex gap-2">
                        <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="px-3 py-1 rounded bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-50 text-sm border border-gray-700">&lt;</button>
                        <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="px-3 py-1 rounded bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-50 text-sm border border-gray-700">&gt;</button>
                    </div>
                </div>
            </div>
            
            <div v-if="!isLoading && payments.length === 0" class="p-10 text-center text-gray-500">
                Kayıt bulunamadı.
            </div>
        </div>

        <PaymentFormModal 
            v-if="showModal"
            :is-open="showModal" 
            :payment-to-edit="selectedPayment || undefined"
            @close="showModal = false"
            @save-success="handleSaveSuccess"
        />

        <IncomeModal 
            v-if="showIncomeModal"
            :is-open="showIncomeModal"
            @close="showIncomeModal = false"
            @save-success="handleSaveSuccess"
        />
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #1e1e2d; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #4b4b60; border-radius: 4px; }
</style>