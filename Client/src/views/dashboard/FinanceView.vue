<script setup lang="ts">
import { ref, computed } from 'vue';

// --- BİLEŞENLER ---
import ExpenseListTable from '../../components/finance/ExpenseListTable.vue';
import PaymentListTable from '../../components/finance/FinanceListTable.vue';
import FinanceSummaryCards from '../../components/finance/FinanceSummaryCards.vue';

// --- TYPES ---
interface FinanceFilters {
    startDate: string;
    endDate: string;
    sortBy: string;
    sortDir: string;
}

// --- STATE ---
const activeTab = ref<string>('expenses');

// useDataTable ile %100 uyumlu filtre yapısı
const filters = ref<FinanceFilters>({
    startDate: '',
    endDate: '',
    sortBy: 'date',
    sortDir: 'desc'
});

// --- COMPUTED ---
// Filtrelerde varsayılan değerden farklı bir şey var mı?
const hasActiveFilters = computed(() => {
    return filters.value.startDate !== '' || 
           filters.value.endDate !== '' || 
           filters.value.sortBy !== 'date' || 
           filters.value.sortDir !== 'desc';
});

// --- ACTIONS ---
const setTab = (tab: string) => {
    activeTab.value = tab;
};

// Sıralama değişimi (V-Model üzerinden daha temiz yönetim)
const onSortSelect = (event: Event) => {
    const target = event.target as HTMLSelectElement;
    const [by, dir] = target.value.split('_');
    filters.value.sortBy = by;
    filters.value.sortDir = dir;
};

// UI/UX: Tek tuşla temizlik
const clearFilters = () => {
    filters.value = {
        startDate: '',
        endDate: '',
        sortBy: 'date',
        sortDir: 'desc'
    };
};
</script>

<template>
    <div class="min-h-screen text-gray-300 font-sans">
        
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
            <div>
                <h1 class="text-2xl font-bold text-white tracking-wide flex items-center gap-2">
                    <span class="text-3xl">💸</span> Finans Yönetimi
                </h1>
                <p class="text-gray-500 text-sm mt-1">Nakit akışınızı, giderlerinizi ve ödemelerinizi tek yerden yönetin.</p>
            </div>
        </div>

        <div class="mb-8">
            <FinanceSummaryCards :filters="filters" />
        </div>

        <div class="bg-[#1E1E2D] p-5 rounded-xl shadow-lg border border-gray-800/50 mb-6">
            <div class="flex flex-col xl:flex-row gap-6 justify-between items-center">
                
                <div class="flex bg-[#151521] p-1.5 rounded-xl w-full xl:w-auto shadow-inner border border-gray-800">
                    <button 
                        @click="setTab('expenses')"
                        :class="['flex-1 xl:flex-none px-8 py-3 rounded-lg text-sm font-bold transition-all flex items-center justify-center gap-2', 
                        activeTab === 'expenses' ? 'bg-[#2B2B40] text-red-400 shadow-lg border border-red-500/20' : 'text-gray-500 hover:text-gray-300 hover:bg-white/5']">
                        📉 Giderler
                    </button>
                    <button 
                        @click="setTab('payments')"
                        :class="['flex-1 xl:flex-none px-8 py-3 rounded-lg text-sm font-bold transition-all flex items-center justify-center gap-2', 
                        activeTab === 'payments' ? 'bg-[#2B2B40] text-emerald-400 shadow-lg border border-emerald-500/20' : 'text-gray-500 hover:text-gray-300 hover:bg-white/5']">
                        📈 Tahsilatlar
                    </button>
                </div>

                <div class="flex flex-wrap items-center gap-4 w-full xl:w-auto justify-end">
                    
                    <transition name="fade">
                        <button 
                            v-if="hasActiveFilters"
                            @click="clearFilters"
                            class="text-red-400 hover:text-red-300 text-sm font-medium px-3 py-2 transition-colors flex items-center gap-1 bg-red-500/10 rounded-lg border border-red-500/20"
                        >
                            <span>✕</span> Temizle
                        </button>
                    </transition>

                    <div class="flex items-center bg-[#151521] rounded-xl border border-gray-700 p-1 shadow-sm focus-within:border-indigo-500 focus-within:ring-1 focus-within:ring-indigo-500 transition-all">
                        <div class="relative group">
                            <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500 pointer-events-none group-focus-within:text-indigo-400 transition-colors">📅</span>
                            <input 
                                type="date" 
                                v-model="filters.startDate" 
                                class="bg-transparent text-white text-sm font-medium pl-10 pr-2 py-2.5 outline-none cursor-pointer [color-scheme:dark] placeholder-gray-600 hover:bg-white/5 rounded-lg transition-colors"
                            >
                        </div>
                        <span class="text-gray-600 px-1 font-bold">-</span>
                        <div class="relative group">
                            <input 
                                type="date" 
                                v-model="filters.endDate" 
                                class="bg-transparent text-white text-sm font-medium px-3 py-2.5 outline-none cursor-pointer [color-scheme:dark] hover:bg-white/5 rounded-lg transition-colors"
                            >
                        </div>
                    </div>

                    <div class="relative">
                        <select 
                            :value="`${filters.sortBy}_${filters.sortDir}`"
                            @change="onSortSelect"
                            class="appearance-none bg-[#151521] text-gray-300 text-sm font-bold py-3 pl-4 pr-10 rounded-xl border border-gray-700 outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 cursor-pointer hover:bg-[#1a1a29] transition-all shadow-sm min-w-[200px]"
                        >
                            <option value="date_desc">📅 Tarih (Yeniden Eskiye)</option>
                            <option value="date_asc">📅 Tarih (Eskiden Yeniye)</option>
                            <option value="amount_desc">💰 Tutar (En Yüksek)</option>
                            <option value="amount_asc">💰 Tutar (En Düşük)</option>
                        </select>
                        <div class="absolute inset-y-0 right-0 flex items-center px-3 pointer-events-none text-gray-500">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" /></svg>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="relative overflow-hidden">
            <transition name="tab-fade" mode="out-in">
                <div :key="activeTab">
                    <div v-if="activeTab === 'expenses'">
                        <ExpenseListTable :filters="filters" />
                    </div>

                    <div v-else-if="activeTab === 'payments'">
                        <PaymentListTable :filters="filters" />
                    </div>
                </div>
            </transition>
        </div>

    </div>
</template>

<style scoped>
/* Tab Geçiş Animasyonu */
.tab-fade-enter-active,
.tab-fade-leave-active {
    transition: all 0.3s ease-out;
}

.tab-fade-enter-from {
    opacity: 0;
    transform: translateY(10px);
}

.tab-fade-leave-to {
    opacity: 0;
    transform: translateY(-10px);
}

.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>