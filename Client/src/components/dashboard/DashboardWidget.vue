<script setup lang="ts">
import { computed } from 'vue';
import {
  Chart as ChartJS, CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler,
  type ChartData, type ChartOptions
} from 'chart.js'
import { Line } from 'vue-chartjs'
import type { DashboardStats } from '../../types'; 
ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend, Filler)

// --- TYPES ---

interface StatCardConfig {
    title: string;
    icon?: string;
    color?: string;
    link?: string;
    // Fonksiyonlar stat objesini alıp değer döndürür
    getValue: (s: DashboardStats) => string | number;
    getSub?: (s: DashboardStats) => string;
    getRate?: (s: DashboardStats) => number;
    getIcon?: (s: DashboardStats) => string;
    getColor?: (s: DashboardStats) => string;
}

interface Props {
    widgetKey: string;
    // Backend'den gelen tüm istatistik verisi
    // (Bazen eksik alanlar olabilir diye Partial yaptık veya direkt DashboardStats kullanabilirsin)
    stats: DashboardStats | any; 
    chartData?: ChartData<'line'>;
    chartOptions?: ChartOptions<'line'>;
}

const props = withDefaults(defineProps<Props>(), {
    chartData: () => ({ datasets: [] }),
    chartOptions: () => ({})
});

const emit = defineEmits<{
    (e: 'navigate', path: string): void;
    (e: 'open-detail', order: any): void;
}>();

const formatCurrency = (value: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);

// --- KART TANIMLARI ---
const statCards: Record<string, StatCardConfig> = {
    'totalRevenue': { 
        title: 'Toplam Ciro', icon: '💰', color: 'primary', 
        getValue: s => formatCurrency(s.totalRevenue), 
        getSub: s => `Net Kâr: ${formatCurrency(s.netProfit)}`,
        getRate: s => s.revenueRate 
    },
    'totalOrders': { 
        title: 'Toplam Sipariş', icon: '📦', color: 'blue', 
        getValue: s => s.totalOrders, 
        getSub: s => 'Adet sipariş alındı', 
        link: '/dashboard/orders',
        getRate: s => s.orderRate
    },
    'totalProducts': { 
        title: 'Toplam Ürün', icon: '🏷️', color: 'cyan', 
        getValue: s => s.totalProducts, 
        getSub: s => 'Çeşit (SKU)', 
        link: '/dashboard/products' 
    },
    'totalCustomers': { 
        title: 'Müşteriler', icon: '👥', color: 'emerald', 
        getValue: s => s.totalCustomers, 
        getSub: s => 'Kayıtlı müşteri sayısı', 
        link: '/dashboard/customers',
        getRate: s => s.customerRate
    },
    'totalStockCount': { 
        title: 'Toplam Stok', icon: '📊', color: 'violet', 
        getValue: s => s.totalStockCount, 
        getSub: s => 'Adet Ürün' 
    },
    
    'criticalStockCount': { 
        title: 'Kritik Stok', 
        getIcon: s => s.criticalStockCount === 0 ? '🛡️' : '⚠️',
        getColor: s => s.criticalStockCount === 0 ? 'emerald' : 'amber',
        getValue: s => s.criticalStockCount, 
        getSub: s => s.criticalStockCount === 0 ? 'Stoklar güvende' : 'Ürün kritik seviyede (<10)',
        link: '/dashboard/products?sort=stock_asc' 
    },
    
    'outOfStockCount': { 
        title: 'Tükenenler', 
        getIcon: s => s.outOfStockCount === 0 ? '✅' : '🚫',
        getColor: s => s.outOfStockCount === 0 ? 'emerald' : 'rose',
        getValue: s => s.outOfStockCount, 
        getSub: s => s.outOfStockCount === 0 ? 'Tüm ürünler stokta' : 'Ürün stokta yok',
        link: '/dashboard/products?sort=stock_asc' 
    }
};

const currentStat = computed(() => statCards[props.widgetKey]);

// Design Tokens uyumlu renkler
const getColorClasses = (color?: string) => {
    const map: Record<string, string> = {
        'primary': 'bg-primary/20 text-indigo-300 ring-1 ring-primary/30 shadow-[0_0_15px_rgba(79,70,229,0.15)]',
        'blue': 'bg-blue-500/20 text-blue-300 ring-1 ring-blue-500/30 shadow-[0_0_15px_rgba(59,130,246,0.15)]',
        'cyan': 'bg-cyan-500/20 text-cyan-300 ring-1 ring-cyan-500/30 shadow-[0_0_15px_rgba(6,182,212,0.15)]',
        'emerald': 'bg-emerald-500/20 text-emerald-300 ring-1 ring-emerald-500/30 shadow-[0_0_15px_rgba(16,185,129,0.15)]',
        'violet': 'bg-violet-500/20 text-violet-300 ring-1 ring-violet-500/30 shadow-[0_0_15px_rgba(139,92,246,0.15)]',
        'amber': 'bg-amber-500/20 text-amber-300 ring-1 ring-amber-500/30 shadow-[0_0_15px_rgba(245,158,11,0.15)]',
        'rose': 'bg-rose-500/20 text-rose-300 ring-1 ring-rose-500/30 shadow-[0_0_15px_rgba(244,63,94,0.15)]',
    };
    return map[color || 'blue'] || map['blue'];
};

const getTrendClasses = (rate?: number) => {
    if (rate === undefined) return '';
    if (rate >= 0) return 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'; 
    return 'bg-rose-500/10 text-rose-400 border-rose-500/20';
};
</script>

<template>
    <div class="h-full select-none touch-none">
        
        <div v-if="currentStat" 
             @click="currentStat.link ? emit('navigate', currentStat.link) : null"
             :class="['relative overflow-hidden p-6 rounded-2xl border flex flex-col justify-between h-full transition-all duration-300 group', 
                      'bg-card/40 backdrop-blur-xl border-card-border hover:border-white/10 hover:shadow-2xl hover:shadow-primary/10 hover:-translate-y-1',
                      currentStat.link ? 'cursor-pointer' : '']">
            
            <div class="absolute bottom-0 left-0 right-0 h-16 opacity-10 pointer-events-none group-hover:opacity-20 transition-opacity">
                 <svg viewBox="0 0 200 60" preserveAspectRatio="none" class="w-full h-full text-white fill-current">
                    <path d="M0,40 Q20,35 40,45 T80,30 T120,40 T160,20 T200,35 V60 H0 Z" />
                 </svg>
            </div>

            <div class="flex items-start justify-between z-10">
                <div>
                    <p class="text-gray-400 text-xs font-bold uppercase tracking-wider mb-1">{{ currentStat.title }}</p>
                    <h3 class="text-3xl font-bold text-white tracking-tight">{{ currentStat.getValue(stats) }}</h3>
                </div>
                <div :class="['p-3 rounded-xl transition-all duration-300', getColorClasses(currentStat.getColor ? currentStat.getColor(stats) : currentStat.color)]">
                    <span class="text-2xl filter drop-shadow-lg">{{ currentStat.getIcon ? currentStat.getIcon(stats) : currentStat.icon }}</span>
                </div>
            </div>

            <div class="mt-4 flex items-center gap-2 z-10">
                <div v-if="currentStat.getRate && currentStat.getRate(stats) !== undefined" 
                     :class="['flex items-center gap-1 text-[10px] font-bold px-1.5 py-0.5 rounded border', getTrendClasses(currentStat.getRate(stats))]">
                    <span>{{ (currentStat.getRate(stats) || 0) >= 0 ? '▲' : '▼' }}</span>
                    <span>%{{ Math.abs(currentStat.getRate(stats) || 0) }}</span>
                </div>
                
                <span class="text-gray-500 text-xs font-medium">{{ currentStat.getSub ? currentStat.getSub(stats) : currentStat.getSub }}</span>
            </div>
        </div>

        <div v-else-if="widgetKey === 'quickActions'" class="bg-gradient-to-br from-primary to-violet-700 p-6 rounded-2xl border border-white/10 shadow-2xl h-full flex flex-col justify-between relative overflow-hidden group">
            <div class="absolute top-0 right-0 p-8 opacity-20 transform translate-x-4 -translate-y-4 group-hover:scale-110 transition-transform duration-500">
                <span class="text-9xl">⚡</span>
            </div>
            
            <div class="z-10">
                <h3 class="text-xl font-bold text-white mb-1">Hızlı İşlemler</h3>
                <p class="text-indigo-200 text-xs">Sık kullandığınız menüler</p>
            </div>

            <div class="grid grid-cols-2 gap-3 mt-4 z-10">
                <button @click="emit('navigate', '/dashboard/orders')" class="bg-white/10 hover:bg-white/20 border border-white/10 p-2 rounded-lg text-left transition-colors">
                    <span class="block text-lg mb-1">📦</span>
                    <span class="text-white text-xs font-bold">Sipariş Ekle</span>
                </button>
                <button @click="emit('navigate', '/dashboard/products')" class="bg-white/10 hover:bg-white/20 border border-white/10 p-2 rounded-lg text-left transition-colors">
                    <span class="block text-lg mb-1">🏷️</span>
                    <span class="text-white text-xs font-bold">Ürün Ekle</span>
                </button>
            </div>
        </div>

        <div v-else-if="widgetKey === 'inventoryValue'" class="relative overflow-hidden p-6 rounded-2xl border border-white/5 h-full flex flex-col justify-center bg-linear-to-br from-card/80 to-bg-dark/80 backdrop-blur-xl">
            <div class="absolute top-0 right-0 p-4 opacity-[0.03]">
                <span class="text-9xl font-serif">₺</span>
            </div>
            <div class="z-10 mb-6">
                <div class="flex items-center gap-2 mb-2">
                    <span class="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse"></span>
                    <p class="text-gray-400 text-xs font-bold uppercase tracking-wider">Toplam Envanter Maliyeti</p>
                </div>
                <h2 class="text-4xl font-black text-transparent bg-clip-text bg-gradient-to-r from-emerald-400 to-cyan-400">
                    {{ formatCurrency(stats.totalInventoryValue) }}
                </h2>
            </div>
            <div class="z-10 pt-4 border-t border-white/5">
                 <p class="text-indigo-300 text-xs font-bold uppercase tracking-wider mb-1">Beklenen Ciro (Potansiyel)</p>
                 <h2 class="text-2xl font-bold text-white">{{ formatCurrency(stats.potentialRevenue) }}</h2>
            </div>
        </div>

        <div v-else-if="widgetKey === 'revenueChart'" class="bg-card/40 backdrop-blur-xl border border-white/5 p-6 rounded-2xl h-full flex flex-col shadow-xl">
            <div class="flex justify-between items-center mb-4">
                <h3 class="text-sm font-bold text-gray-200 uppercase tracking-wide">Gelir Analizi</h3>
                <span class="text-xs text-gray-500 bg-input/50 px-2 py-1 rounded">Satış Trendi</span>
            </div>
            <div class="flex-1 relative w-full h-full min-h-0">
                <Line :data="chartData" :options="chartOptions" />
            </div>
        </div>

        <div v-else-if="widgetKey === 'topSelling'" class="bg-card/40 backdrop-blur-xl border border-white/5 p-6 rounded-2xl h-full flex flex-col shadow-xl">
            <h3 class="text-sm font-bold text-gray-200 uppercase tracking-wide mb-6 flex items-center gap-2">
                <span class="text-lg">🏆</span> En Çok Satanlar
            </h3>
            
            <div v-if="!stats.topSellingProducts || stats.topSellingProducts.length === 0" class="text-gray-500 text-center my-auto text-sm">
                Henüz veri yok.
            </div>
            
            <div v-else class="space-y-5 grow overflow-y-auto custom-scrollbar pr-2">
                <div v-for="(prod, index) in stats.topSellingProducts" :key="index" class="group/item">
                    <div class="flex justify-between text-sm mb-2">
                        <span class="text-white font-medium truncate max-w-[70%] group-hover/item:text-indigo-400 transition-colors">{{ prod.name }}</span>
                        <span class="text-gray-400 font-mono text-xs bg-gray-700/50 px-1.5 py-0.5 rounded">{{ prod.salesCount }} Adet</span>
                    </div>
                    <div class="w-full bg-gray-700/30 rounded-full h-1.5 overflow-hidden">
                        <div class="bg-primary h-full rounded-full transition-all duration-1000 ease-out shadow-[0_0_10px_rgba(79,70,229,0.5)]" 
                             :style="{ width: prod.percentage + '%' }"></div>
                    </div>
                </div>
            </div>
            
            <button @click="emit('navigate', '/dashboard/products')" class="mt-6 text-xs font-bold text-center text-indigo-400 hover:text-indigo-300 border border-primary/20 hover:border-primary/50 hover:bg-primary/10 py-3 rounded-xl transition-all uppercase tracking-wider w-full">
                Ürünleri Yönet
            </button>
        </div>

        <div v-else-if="widgetKey === 'recentOrders'" class="bg-card/40 backdrop-blur-xl border border-white/5 rounded-2xl shadow-xl overflow-hidden h-full flex flex-col">
            <div class="p-6 border-b border-white/5 flex justify-between items-center shrink-0">
                <h3 class="text-sm font-bold text-gray-200 uppercase tracking-wide">Son Siparişler</h3>
                <button @click="emit('navigate', '/dashboard/orders')" class="text-xs font-medium text-indigo-400 hover:text-indigo-300">Tümünü Gör</button>
            </div>
            <div class="overflow-x-auto grow custom-scrollbar">
                <div v-if="!stats.recentOrders || stats.recentOrders.length === 0" class="p-8 text-center text-gray-500 text-sm">Henüz hiç sipariş alınmamış.</div>
                <table v-else class="w-full text-left border-collapse">
                    <thead>
                        <tr class="bg-gray-900/20 text-gray-500 text-[10px] font-bold uppercase tracking-wider">
                            <th class="p-4">ID</th>
                            <th class="p-4">Müşteri</th>
                            <th class="p-4">Tutar</th>
                            <th class="p-4 text-center">Durum</th>
                            <th class="p-4 text-right">#</th> 
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-white/5">
                        <tr v-for="order in stats.recentOrders" :key="order.id" class="hover:bg-white/5 transition-colors group/row">
                            <td class="p-4 text-indigo-400 font-mono text-xs">#{{ order.id }}</td>
                            <td class="p-4 text-gray-200 text-sm font-medium">
                                {{ order.customerName || 'Misafir' }}
                            </td>
                            <td class="p-4 text-gray-300 font-medium text-sm">{{ formatCurrency(order.amount) }}</td>
                            <td class="p-4 text-center">
                                <span :class="{'px-2 py-1 rounded text-[10px] font-bold uppercase border shadow-sm': true,
                                    'bg-emerald-500/10 text-emerald-400 border-emerald-500/20 shadow-emerald-500/10': order.status === 'Tamamlandı' || order.status === 'Ödendi',
                                    'bg-amber-500/10 text-amber-400 border-amber-500/20 shadow-amber-500/10': order.status === 'Kısmi',
                                    'bg-rose-500/10 text-rose-400 border-rose-500/20 shadow-rose-500/10': order.status === 'Bekliyor'}">
                                    {{ order.status }}
                                </span>
                            </td>
                            <td class="p-4 text-right">
                                <button @click="emit('open-detail', order)" class="text-gray-500 hover:text-white transition-colors p-1 hover:bg-gray-700 rounded">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" /></svg>
                                </button>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

    </div>
</template>