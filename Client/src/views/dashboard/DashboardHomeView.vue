<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue'; 
import { useRouter } from 'vue-router'; 
import api from '../../services/api';
import { useToast } from 'vue-toastification';
import draggable from 'vuedraggable';

// BİLEŞENLER
import DashboardWidget from '../../components/dashboard/DashboardWidget.vue';
import DashboardCustomizeModal from '../../components/dashboard/DashboardCustomizeModal.vue';
import DashboardOrderDetailModal from '../../components/dashboard/DashboardOrderDetailModal.vue';

const router = useRouter();
const toast = useToast();

// --- TYPES ---
interface DashboardStats {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  totalProducts: number;
  netProfit: number;
  totalStockCount: number;
  criticalStockCount: number;
  outOfStockCount: number;
  totalInventoryValue: number;
  potentialRevenue: number;
  topSellingProducts: any[];
  recentOrders: any[];
  revenueRate: number;
  orderRate: number;
  customerRate: number;
  // Yeni Alanlar (Default)
  totalShippingCost: number;
  totalOperationalExpenses: number;
  [key: string]: any; // API'den gelen dinamik veriler için
}

interface ChartDataset {
  label: string;
  backgroundColor: string;
  borderColor: string;
  borderWidth: number;
  fill: boolean;
  tension: number;
  data: number[];
}

interface ChartData {
  labels: string[];
  datasets: ChartDataset[];
}

interface DateOption {
  label: string;
  value: string;
}

// --- STATE ---
const stats = ref<DashboardStats>({
  totalRevenue: 0, totalOrders: 0, totalCustomers: 0, totalProducts: 0, 
  netProfit: 0, totalStockCount: 0, criticalStockCount: 0, 
  outOfStockCount: 0, totalInventoryValue: 0, potentialRevenue: 0,
  topSellingProducts: [], recentOrders: [],
  revenueRate: 0, orderRate: 0, customerRate: 0,
  // Yeni Alanlar (Default)
  totalShippingCost: 0, totalOperationalExpenses: 0
});

const chartData = ref<ChartData>({ labels: [], datasets: [] });
const chartOptions = { 
    responsive: true, 
    maintainAspectRatio: false, 
    plugins: { legend: { display: false } }, 
    scales: { 
        x: { grid: { display: false }, ticks: { color: '#9ca3af' } }, 
        y: { grid: { color: '#374151', borderDash: [5, 5] }, ticks: { color: '#9ca3af' } } 
    } 
};

const isLoading = ref<boolean>(true);
const showCustomizeModal = ref<boolean>(false);
const showDetailModal = ref<boolean>(false);
const selectedOrder = ref<any>(null);

// EDİT MODU
const isEditMode = ref<boolean>(false);
const originalOrder = ref<string[]>([]); 
const activeWidgetKeys = ref<string[]>([]); 

// Varsayılan Sıralama (Mevcut Widgetlar)
const defaultWidgets: string[] = [
    'quickActions',
    'totalProducts', 'totalStockCount', 'criticalStockCount', 'outOfStockCount',
    'inventoryValue', 'revenueChart',
    'topSelling', 'recentOrders'
];

// --- FİNANSAL KART HESAPLAMALARI (YENİ) ---
const formatCurrency = (val: number): string => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val || 0);

// 1. Toplam Ciro
const cardRevenue = computed(() => stats.value.totalRevenue || 0);

// 2. Kargo Gideri
const cardShipping = computed(() => stats.value.totalShippingCost || 0);

// 3. Toplam Kesinti (Kargo HARİÇ -> Komisyon + Vergi)
const cardDeductions = computed(() => {
    const totalOps = stats.value.totalOperationalExpenses || 0;
    const shipping = stats.value.totalShippingCost || 0;
    return totalOps - shipping;
});

// 4. Net Kazanç
const cardNetProfit = computed(() => stats.value.netProfit || 0);


// --- TARİH FİLTRESİ AYARLARI ---
const dateRange = ref<string>('thisMonth'); 

const dateOptions: DateOption[] = [
    { label: '📅 Bugün', value: 'today' },
    { label: '📅 Bu Hafta', value: 'thisWeek' },
    { label: '📅 Bu Ay', value: 'thisMonth' },
    { label: '📅 Tüm Zamanlar', value: 'all' }
];

const getDateRangeParams = (): { startDate: string | null; endDate: string } => {
    const now = new Date();
    let end = new Date(); 
    let start: Date | null = null;

    if (dateRange.value === 'today') {
        start = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    } else if (dateRange.value === 'thisWeek') {
        const day = now.getDay() || 7; 
        now.setDate(now.getDate() - (day - 1));
        now.setHours(0, 0, 0, 0); 
        start = now;
    } else if (dateRange.value === 'thisMonth') {
        start = new Date(now.getFullYear(), now.getMonth(), 1);
    } 
    
    return { 
        startDate: start ? start.toISOString() : null, 
        endDate: end.toISOString() 
    };
};

// --- GRID AYARLARI ---
const getGridSpan = (key: string): string => {
    switch (key) {
        case 'revenueChart': return 'md:col-span-2 lg:col-span-2 row-span-2 h-105'; 
        case 'recentOrders': return 'md:col-span-2 lg:col-span-2 row-span-2 h-105'; 
        case 'inventoryValue': return 'md:col-span-1 lg:col-span-1 row-span-1 h-50';
        case 'topSelling': return 'md:col-span-1 lg:col-span-1 row-span-2 h-105';
        case 'quickActions': return 'md:col-span-1 lg:col-span-1 row-span-1 h-50';
        default: return 'md:col-span-1 lg:col-span-1 row-span-1 h-40'; 
    }
};

const fetchData = async () => {
    isLoading.value = true;
    try {
        const { startDate, endDate } = getDateRangeParams();
        const data: any = await api.getDashboardData(startDate as string, endDate); 
        stats.value = data;
        
        chartData.value = { 
            labels: data.last6MonthsLabels || [], 
            datasets: [{ 
                label: 'Dönem Geliri (TL)', 
                backgroundColor: 'rgba(79, 70, 229, 0.2)',
                borderColor: '#4F46E5',
                borderWidth: 2, 
                fill: true, 
                tension: 0.4, 
                data: data.last6MonthsRevenue || [] 
            }] 
        };
    } catch (error) { 
        console.error("Dashboard verisi çekilemedi:", error); 
        toast.error("Veriler güncellenemedi.");
    } finally { 
        setTimeout(() => isLoading.value = false, 300);
    }
};

onMounted(async () => {
    await fetchData();
    try {
        const settings = await api.getDashboardSettings();
        if (settings && Array.isArray(settings) && settings.length > 0) activeWidgetKeys.value = settings;
        else activeWidgetKeys.value = defaultWidgets;
    } catch (e) {
        activeWidgetKeys.value = defaultWidgets;
    }
});

watch(dateRange, () => { fetchData(); });

const saveSettings = async (newKeys: string[], showToast = true) => {
    try {
        activeWidgetKeys.value = newKeys; 
        await api.saveDashboardSettings(newKeys);
        if (showToast) toast.success("Düzen başarıyla kaydedildi 👍");
    } catch (err) {
        toast.error("Ayarlar kaydedilemedi.");
    }
};

const toggleEditMode = () => {
    if (!isEditMode.value) {
        originalOrder.value = [...activeWidgetKeys.value];
        isEditMode.value = true;
    } else {
        isEditMode.value = false;
        const hasChanged = JSON.stringify(originalOrder.value) !== JSON.stringify(activeWidgetKeys.value);
        if (hasChanged) {
            saveSettings(activeWidgetKeys.value, true);
        }
    }
};

const onDragEnd = () => {
    console.log('Yeni sıralama (Henüz kaydedilmedi):', activeWidgetKeys.value);
};
const handleNavigateWithCheck = (path: string) => { if (!isEditMode.value) router.push(path); };
const openOrderDetail = (order: any) => { if (!isEditMode.value) { selectedOrder.value = order; showDetailModal.value = true; } };
const goToOrdersWithHighlight = () => { if (selectedOrder.value) router.push({ path: '/dashboard/orders', query: { highlight: selectedOrder.value.id } }); };
</script>

<template>
  <div class="pb-10">
    
    <div class="mb-8 flex flex-col md:flex-row justify-between items-end gap-4">
        <div>
            <h1 class="text-2xl font-bold text-white">Gösterge Paneli</h1>
            <p class="text-gray-400 text-sm">İşletmenizin anlık finansal özeti.</p>
        </div>
        
        <div class="flex items-center gap-3">
            <div class="relative">
                <select v-model="dateRange" class="appearance-none bg-input text-white text-sm border border-input-border hover:border-gray-600 px-4 py-2 pr-8 rounded-lg cursor-pointer outline-none focus:ring-2 focus:ring-primary/50 transition-all">
                    <option v-for="opt in dateOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
                </select>
                <div class="absolute inset-y-0 right-0 flex items-center px-2 pointer-events-none text-gray-400">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                </div>
            </div>

            <button @click="toggleEditMode" :class="['text-sm px-4 py-2 rounded-lg border transition-all flex items-center gap-2 font-medium', isEditMode ? 'bg-primary text-white border-primary shadow-lg' : 'bg-input text-gray-300 border-input-border hover:bg-gray-700']">
                <span v-if="isEditMode">💾 Kaydet</span>
                <span v-else>✏️ Düzenle</span>
            </button>

            <button @click="showCustomizeModal = true" class="text-sm bg-input hover:bg-gray-700 text-gray-300 px-3 py-2 rounded-lg border border-input-border transition-colors">
                ⚙️
            </button>
        </div>
    </div>

    <div v-if="!isLoading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
        <div class="bg-[#1E1E2D] p-5 rounded-2xl border border-gray-700/50 shadow-lg relative overflow-hidden group hover:border-blue-500/30 transition-all">
            <div class="absolute right-0 top-0 w-24 h-24 bg-blue-500/5 rounded-full blur-xl -mr-6 -mt-6"></div>
            <div class="relative z-10">
                <div class="flex justify-between items-start mb-2">
                    <span class="text-gray-400 text-xs font-bold uppercase tracking-wider">Toplam Ciro</span>
                    <span class="text-xl">💰</span>
                </div>
                <div class="text-2xl font-black text-white font-mono">{{ formatCurrency(cardRevenue) }}</div>
                <div class="text-xs text-blue-400 mt-1 font-medium">Satış Geliri</div>
            </div>
        </div>

        <div class="bg-[#1E1E2D] p-5 rounded-2xl border border-gray-700/50 shadow-lg relative overflow-hidden group hover:border-orange-500/30 transition-all">
            <div class="absolute right-0 top-0 w-24 h-24 bg-orange-500/5 rounded-full blur-xl -mr-6 -mt-6"></div>
            <div class="relative z-10">
                <div class="flex justify-between items-start mb-2">
                    <span class="text-gray-400 text-xs font-bold uppercase tracking-wider">Toplam Kesinti</span>
                    <span class="text-xl">✂️</span>
                </div>
                <div class="text-2xl font-black text-white font-mono">{{ formatCurrency(cardDeductions) }}</div>
                <div class="text-xs text-orange-400 mt-1 font-medium">Komisyon + KDV</div>
            </div>
        </div>

        <div class="bg-[#1E1E2D] p-5 rounded-2xl border border-gray-700/50 shadow-lg relative overflow-hidden group hover:border-red-500/30 transition-all">
            <div class="absolute right-0 top-0 w-24 h-24 bg-red-500/5 rounded-full blur-xl -mr-6 -mt-6"></div>
            <div class="relative z-10">
                <div class="flex justify-between items-start mb-2">
                    <span class="text-gray-400 text-xs font-bold uppercase tracking-wider">Kargo Gideri</span>
                    <span class="text-xl">🚚</span>
                </div>
                <div class="text-2xl font-black text-white font-mono">{{ formatCurrency(cardShipping) }}</div>
                <div class="text-xs text-red-400 mt-1 font-medium">Lojistik Maliyeti</div>
            </div>
        </div>

        <div class="bg-gradient-to-br from-emerald-600 to-emerald-800 p-5 rounded-2xl shadow-xl text-white relative overflow-hidden group">
            <div class="absolute right-0 top-0 w-32 h-32 bg-white/10 rounded-full blur-3xl -mr-10 -mt-10 pointer-events-none"></div>
            <div class="relative z-10">
                <div class="flex justify-between items-start mb-2">
                    <span class="text-emerald-100 text-xs font-bold uppercase tracking-wider">Net Kazanç</span>
                    <span class="text-xl bg-white/20 p-1 rounded">💵</span>
                </div>
                <div class="text-3xl font-black font-mono tracking-tight">{{ formatCurrency(cardNetProfit) }}</div>
                <div class="text-xs text-emerald-200 mt-1 font-medium">Cepte Kalan</div>
            </div>
        </div>
    </div>

    <draggable 
        v-if="!isLoading"
        v-model="activeWidgetKeys" 
        tag="div"
        :style="isEditMode ? 'background-image: radial-gradient(rgba(79, 70, 229, 0.15) 1px, transparent 1px); background-size: 24px 24px;' : ''"
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 grid-flow-row-dense auto-rows-min rounded-2xl transition-all duration-300"
        item-key="key => key"
        :animation="300" 
        :disabled="!isEditMode"
        ghost-class="opacity-20"
        @end="onDragEnd"
    >
        <template #item="{ element: key }">
            <div :class="[getGridSpan(key), 'transition-all duration-200 rounded-2xl h-full', isEditMode ? 'cursor-move ring-2 ring-primary ring-offset-4 ring-offset-bg-dark scale-[0.98] z-20' : 'cursor-default']">
                <div class="h-full" :class="{ 'pointer-events-none': isEditMode }">
                    <DashboardWidget 
                        :widget-key="key"
                        :stats="stats"
                        :chart-data="chartData"
                        :chart-options="chartOptions"
                        @navigate="handleNavigateWithCheck"
                        @open-detail="openOrderDetail"
                    />
                </div>
                <div v-if="isEditMode" class="absolute top-2 right-2 z-30 bg-primary rounded-full p-1.5 shadow-lg pointer-events-none animate-bounce">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" /></svg>
                </div>
            </div>
        </template>
    </draggable>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 grid-flow-row-dense">
         <div class="md:col-span-1 lg:col-span-1 h-40 bg-card/40 rounded-2xl animate-pulse border border-card-border"></div>
         <div v-for="i in 3" :key="i" class="md:col-span-1 lg:col-span-1 h-40 bg-card/40 rounded-2xl animate-pulse border border-card-border"></div>
         
         <div class="md:col-span-1 lg:col-span-1 h-50 bg-card/40 rounded-2xl animate-pulse border border-card-border"></div>
         <div class="md:col-span-2 lg:col-span-2 h-105 bg-card/40 rounded-2xl animate-pulse border border-card-border"></div>
         <div class="md:col-span-1 lg:col-span-1 h-105 bg-card/40 rounded-2xl animate-pulse border border-card-border"></div>

         <div class="md:col-span-2 lg:col-span-2 h-105 bg-card/40 rounded-2xl animate-pulse border border-card-border"></div>
    </div>

    <DashboardCustomizeModal :is-open="showCustomizeModal" :initial-settings="activeWidgetKeys" @close="showCustomizeModal = false" @save="saveSettings" />
    <DashboardOrderDetailModal :is-open="showDetailModal" :order="selectedOrder" @close="showDetailModal = false" @navigate="goToOrdersWithHighlight" />

  </div>
</template>