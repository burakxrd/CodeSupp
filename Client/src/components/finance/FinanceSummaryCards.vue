<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import api from '../../services/api';
import { formatCurrency } from '../../utils/formatters';

// --- TYPES ---

// Parent'tan gelen filtre objesinin sadece BU bileşeni ilgilendiren kısımlarını tanımlıyoruz.
// Fazladan gelen (sortBy vs.) alanlar TypeScript'i bozmaz (Duck Typing).
interface FinanceFilters {
    startDate: string;
    endDate: string;
}

// Backend'den dönen özet veri yapısı
interface FinanceSummary {
    totalIncome: number;
    totalExpense: number;
    netBalance: number;
}

// --- PROPS ---
interface Props {
    filters: FinanceFilters;
}

const props = defineProps<Props>();

// --- STATE ---
const stats = ref<FinanceSummary>({
    totalIncome: 0,
    totalExpense: 0,
    netBalance: 0
});

const isLoading = ref<boolean>(false);

// --- ACTIONS ---
const fetchSummary = async () => {
    // Tarihler boşsa API'ye gitme veya varsayılan davranışı kontrol et
    // (Opsiyonel: if (!props.filters.startDate) return;)
    
    try {
        isLoading.value = true;
        const res = await api.getFinanceSummary(props.filters.startDate, props.filters.endDate);
        // Backend'den dönen veriyi güvenli şekilde cast ediyoruz
        stats.value = res as FinanceSummary;
    } catch (err) {
        console.error("Özet yüklenirken hata:", err);
    } finally {
        isLoading.value = false;
    }
};

// Filtrelerdeki tarih değişirse yeniden hesapla
watch(
    () => [props.filters.startDate, props.filters.endDate], 
    () => {
        fetchSummary();
    }, 
    { deep: true } // Nested obje olduğu için deep izleme iyidir
);

onMounted(() => {
    fetchSummary();
});
</script>

<template>
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 w-full">
        
        <div class="bg-[#1E1E2D] p-5 rounded-xl border border-gray-800 shadow-lg flex items-center justify-between relative overflow-hidden group">
            <div class="relative z-10">
                <p class="text-gray-400 text-sm font-medium mb-1">Toplam Tahsilat</p>
                <h3 class="text-2xl font-bold text-emerald-400 font-mono">
                    <span v-if="isLoading" class="animate-pulse">...</span>
                    <span v-else>+{{ formatCurrency(stats.totalIncome) }}</span>
                </h3>
            </div>
            <div class="bg-emerald-500/10 p-3 rounded-lg text-emerald-500 group-hover:scale-110 transition-transform">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" /></svg>
            </div>
            <div class="absolute -bottom-4 -right-4 w-24 h-24 bg-emerald-500/5 rounded-full blur-2xl group-hover:bg-emerald-500/10 transition-colors"></div>
        </div>

        <div class="bg-[#1E1E2D] p-5 rounded-xl border border-gray-800 shadow-lg flex items-center justify-between relative overflow-hidden group">
            <div class="relative z-10">
                <p class="text-gray-400 text-sm font-medium mb-1">Toplam Gider</p>
                <h3 class="text-2xl font-bold text-red-400 font-mono">
                    <span v-if="isLoading" class="animate-pulse">...</span>
                    <span v-else>-{{ formatCurrency(stats.totalExpense) }}</span>
                </h3>
            </div>
            <div class="bg-red-500/10 p-3 rounded-lg text-red-500 group-hover:scale-110 transition-transform">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 17h8m0 0V9m0 8l-8-8-4 4-6-6" /></svg>
            </div>
             <div class="absolute -bottom-4 -right-4 w-24 h-24 bg-red-500/5 rounded-full blur-2xl group-hover:bg-red-500/10 transition-colors"></div>
        </div>

        <div class="bg-[#1E1E2D] p-5 rounded-xl border border-gray-800 shadow-lg flex items-center justify-between relative overflow-hidden group">
            <div class="relative z-10">
                <p class="text-gray-400 text-sm font-medium mb-1">Net Durum</p>
                <h3 :class="['text-2xl font-bold font-mono', stats.netBalance >= 0 ? 'text-blue-400' : 'text-orange-400']">
                    <span v-if="isLoading" class="animate-pulse">...</span>
                    <span v-else>{{ stats.netBalance >= 0 ? '+' : '' }}{{ formatCurrency(stats.netBalance) }}</span>
                </h3>
            </div>
            <div :class="['p-3 rounded-lg group-hover:scale-110 transition-transform', stats.netBalance >= 0 ? 'bg-blue-500/10 text-blue-500' : 'bg-orange-500/10 text-orange-500']">
                <svg v-if="stats.netBalance >= 0" xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 6l3 1m0 0l-3 9a5.002 5.002 0 006.001 0M6 7l3 9M6 7l6-2m6 2l3-1m-3 1l-3 9a5.002 5.002 0 006.001 0M18 7l3 9m-3-9l-6-2m0-2v2m0 16V5m0 16H9m3 0h3" /></svg>
                <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>
            </div>
             <div class="absolute -bottom-4 -right-4 w-24 h-24 bg-blue-500/5 rounded-full blur-2xl group-hover:bg-blue-500/10 transition-colors"></div>
        </div>

    </div>
</template>