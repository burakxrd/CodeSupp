<script setup lang="ts">
import { ref, onMounted, toRef, defineAsyncComponent } from 'vue';
import api from '../../services/api';
import { useDataTable } from '../../composables/useDataTable';
import { formatCurrency, formatDateTime } from '../../utils/formatters';
import type { Expense } from '../../types'; // Global tipler

// --- TYPES ---
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

// --- MODAL (Lazy Load) ---
const ExpenseFormModal = defineAsyncComponent(() => 
  import('./ExpenseFormModal.vue')
);

// --- ADAPTER ---
// API'den gelen veriyi Composable'ın beklediği formata çevirir
const fetchExpensesAdapter = async (p: any) => {
    const res: any = await api.getExpenses(
        p.page,
        p.pageSize,
        p.search,
        props.filters.sortBy,
        props.filters.sortDir,
        props.filters.startDate || undefined, // null -> undefined
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
// handleSearch'i manuel ekleyeceğiz
const {
    items: expenses,
    isLoading,
    searchQuery,
    currentPage,
    totalPages,
    totalCount,
    pageSize,
    fetchData: fetchExpenses,
    changePage,
    handleDelete
} = useDataTable<Expense>({
    apiFetch: fetchExpensesAdapter,
    apiDelete: (id) => api.deleteExpense(Number(id)),
    filters: toRef(props, 'filters'),
    pageSizeInit: 10
});

// MANUEL SEARCH (Debounce ile)
let searchTimeout: ReturnType<typeof setTimeout>;
const handleSearch = (e: Event) => {
    const val = (e.target as HTMLInputElement).value;
    searchQuery.value = val;
    
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        fetchExpenses();
    }, 300);
};

// --- MODAL STATE ---
const showModal = ref<boolean>(false);
const selectedExpense = ref<Expense | null>(null);

const openAddModal = () => {
    selectedExpense.value = null; 
    showModal.value = true;
};

const openEditModal = (expense: Expense) => {
    selectedExpense.value = { ...expense };
    showModal.value = true;
};

const handleSaveSuccess = () => {
    fetchExpenses(currentPage.value);
};

// Başlangıç yüklemesi
onMounted(() => {
    fetchExpenses();
});
</script>

<template>
    <div class="space-y-6">
        <div class="flex flex-col md:flex-row gap-4 justify-between items-center">
            <div class="relative w-full md:w-1/3">
                <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500">🔍</span>
                <input 
                    :value="searchQuery" 
                    @input="handleSearch"
                    type="text" 
                    placeholder="Gider açıklaması ara..." 
                    class="w-full bg-[#151521] text-gray-300 border border-gray-700 rounded-xl py-2.5 pl-10 pr-4 focus:border-red-500/50 outline-none transition-all text-sm"
                >
            </div>

            <button 
                @click="openAddModal" 
                class="w-full md:w-auto bg-red-600 hover:bg-red-500 text-white px-6 py-2.5 rounded-xl font-bold text-sm shadow-lg shadow-red-500/20 flex items-center justify-center gap-2 transition-all transform hover:scale-105 active:scale-95"
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
                </svg>
                Yeni Gider Ekle
            </button>
        </div>

        <div class="bg-[#1E1E2D] rounded-xl shadow-xl border border-gray-800/50 flex flex-col min-h-[450px] relative overflow-hidden">
            
            <div v-if="isLoading" class="absolute inset-0 bg-[#1E1E2D]/50 backdrop-blur-sm z-10 flex items-center justify-center">
                <div class="flex flex-col items-center gap-3">
                    <div class="w-10 h-10 border-4 border-red-500/20 border-t-red-500 rounded-full animate-spin"></div>
                    <span class="text-sm text-gray-400 font-medium">Yükleniyor...</span>
                </div>
            </div>
            
            <div class="flex-1 flex flex-col">
                <div class="overflow-x-auto">
                    <table class="w-full text-left border-collapse min-w-[700px]">
                        <thead>
                            <tr class="bg-[#151521] text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-800">
                                <th class="p-5 pl-8">Tarih</th>
                                <th class="p-5">Açıklama</th>
                                <th class="p-5 text-right">Tutar</th>
                                <th class="p-5 text-right pr-8">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-800">
                            <tr v-for="expense in expenses" :key="expense.id" class="group hover:bg-[#2B2B40]/50 transition-colors">
                                <td class="p-5 pl-8 text-gray-400 font-medium text-sm">
                                    {{ formatDateTime(expense.date) }}
                                </td>
                                <td class="p-5">
                                    <div class="text-white font-medium text-base">{{ expense.description }}</div>
                                    <div class="text-[10px] text-gray-600 uppercase tracking-tighter mt-0.5">ID: #{{ expense.id }}</div>
                                </td>
                                <td class="p-5 text-right font-mono text-lg font-bold text-red-400">
                                    -{{ formatCurrency(expense.amount) }}
                                </td>
                                <td class="p-5 text-right pr-8">
                                    <div class="flex items-center justify-end gap-2">
                                        <button 
                                            @click="openEditModal(expense)" 
                                            class="p-2 rounded-lg bg-blue-500/10 text-blue-400 hover:bg-blue-500 hover:text-white transition-all"
                                            title="Düzenle"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                                            </svg>
                                        </button>
                                        <button 
                                            @click="handleDelete(expense.id, 'Bu gider kaydını silmek istediğinize emin misiniz?')" 
                                            class="p-2 rounded-lg bg-red-500/10 text-red-400 hover:bg-red-500 hover:text-white transition-all"
                                            title="Sil"
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                            </svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

                <div v-if="!isLoading && expenses.length === 0" class="flex-1 flex flex-col items-center justify-center p-10 text-center">
                    <div class="w-16 h-16 bg-gray-800 rounded-full flex items-center justify-center mb-4 text-2xl">📉</div>
                    <p class="text-gray-500 font-medium">Kayıtlı gider bulunamadı.</p>
                </div>

                <div v-if="totalPages > 1" class="p-4 border-t border-gray-800 flex items-center justify-between bg-[#151521] mt-auto">
                    <span class="text-xs text-gray-500">
                        Toplam <span class="text-gray-300 font-bold">{{ totalCount }}</span> kayıttan 
                        {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} arası
                    </span>
                    <div class="flex items-center gap-1">
                        <button 
                            @click="changePage(currentPage - 1)" 
                            :disabled="currentPage === 1" 
                            class="p-2 rounded-lg bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed border border-gray-700 transition-colors"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                            </svg>
                        </button>
                        <div class="px-3 py-1 text-sm font-bold text-white bg-indigo-600 rounded-lg shadow-lg shadow-indigo-500/20">
                            {{ currentPage }}
                        </div>
                        <button 
                            @click="changePage(currentPage + 1)" 
                            :disabled="currentPage === totalPages" 
                            class="p-2 rounded-lg bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-30 disabled:cursor-not-allowed border border-gray-700 transition-colors"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                            </svg>
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <ExpenseFormModal 
            v-if="showModal"
            :is-open="showModal" 
            :expense-to-edit="selectedExpense || undefined"
            @close="showModal = false"
            @save-success="handleSaveSuccess"
        />
    </div>
</template>