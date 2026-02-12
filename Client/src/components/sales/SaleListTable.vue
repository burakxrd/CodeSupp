<script setup lang="ts">
import { ref, nextTick, watch } from 'vue';
import { useSettingsStore } from '../../stores/settings';
import { useTooltip } from '../../composables/forms/useTooltip';
import { formatCurrency, formatDate, calculateNetProfit, calculateTotalDeductions } from '../../utils/formatters';

// --- TYPES ---
interface SaleProduct {
    name: string;
    quantity: number;
    [key: string]: any;
}

interface Sale {
    id: number;
    customerId?: number | null; 
    customerName?: string;
    saleDate?: string;         
    totalAmount?: number;      
    remainingAmount?: number;   
    paymentStatus?: string;    
    shippingStatus?: string;
    products?: SaleProduct[]; 
    customerPhone?: string;
    customerEmail?: string;
    customerAddress?: string;
    rowVersion?: string;
    [key: string]: any;
}

interface Props {
    sales: Sale[];
    isLoading?: boolean;
    highlightId?: number | string | null;
    totalCount?: number;
    currentPage?: number;
    totalPages?: number;
    pageSize?: number;
}

// --- STORE ---
const settingsStore = useSettingsStore();

// --- PROPS & EMITS ---
const props = withDefaults(defineProps<Props>(), {
    sales: () => [],
    isLoading: false,
    highlightId: null,
    totalCount: 0,
    currentPage: 1,
    totalPages: 1,
    pageSize: 10
});

const emit = defineEmits<{
    (e: 'page-change', page: number): void;
    (e: 'pay', sale: Sale): void;
    (e: 'delete', id: number): void;
    (e: 'update-status', payload: { id: number, status: number }): void;
}>();

// --- LOCAL STATE (UI State) ---
const expandedRows = ref<Record<number, boolean>>({}); 

const toggleRowExpansion = (id: number) => {
    expandedRows.value[id] = !expandedRows.value[id];
};

// --- TOOLTIP LOGIC ---
const { tooltip, showTooltip, hideTooltip } = useTooltip();

// --- KARGO İŞLEMLERİ ---
const shippingOptions = [
    { label: 'Sipariş Alındı', value: 1 },  
    { label: 'Hazırlanıyor', value: 2 },    
    { label: 'Kargoya Verildi', value: 3 },  
    { label: 'Teslim Edildi', value: 4 },   
    { label: 'İptal Edildi', value: 5 },     
    { label: 'İade Edildi', value: 6 }       
];

const onStatusChange = (sale: Sale, newStatus: string | number) => {
    emit('update-status', { id: sale.id, status: Number(newStatus) });
};

// --- HIGHLIGHT ---
watch(() => props.highlightId, (newId) => {
    if (newId) {
        nextTick(() => {
            const el = document.getElementById(`order-${newId}`);
            if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        });
    }
});
</script>

<template>
    <div class="bg-[#1E1E2D] rounded-xl shadow-xl border border-gray-800/50 overflow-hidden flex flex-col">
        
        <div v-if="isLoading" class="p-10 text-center text-gray-500 text-lg">Yükleniyor...</div>

        <div v-else>
            <div class="overflow-x-auto">
                <table class="w-full text-left border-collapse min-w-[1200px]">
                    <thead>
                        <tr class="bg-[#151521] text-gray-400 text-sm font-bold uppercase tracking-wider border-b border-gray-800">
                            <th class="p-6 pl-8 w-[8%]">ID</th>
                            <th class="p-6 w-[20%]">Müşteri</th>
                            <th class="p-6 w-[20%]">Ürünler</th> 
                            <th class="p-6 w-[10%]">Tarih</th>
                            <th class="p-6 text-right w-[10%]">Tutar</th>
                            <th class="p-6 text-right w-[10%] text-emerald-500/80">Net Kazanç</th>
                            <th class="p-6 text-center w-[10%]">Tahsilat Durumu</th> 
                            <th v-if="settingsStore.showShippingColumn" class="p-6 w-[15%]">Kargo Durumu</th>
                            <th class="p-6 text-right pr-8 w-[10%]">İşlemler</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-800">
                        <tr 
                            v-for="sale in sales" 
                            :key="sale.id" 
                            :id="`order-${sale.id}`" 
                            class="group transition-all duration-500 border-l-4 border-transparent"
                            :class="{ 
                                'highlight-glow z-10 relative': highlightId == sale.id,
                                'hover:bg-[#2B2B40]': highlightId != sale.id 
                            }"
                        >
                            <td class="p-6 pl-8 text-indigo-400 font-mono text-base font-bold align-top">#{{ sale.id }}</td>
                            
                            <td class="p-6 align-top">
                                <div class="flex items-center">
                                    <div class="h-9 w-9 rounded-lg bg-gray-700 flex items-center justify-center text-sm font-bold text-white mr-3 uppercase shadow-lg shadow-gray-700/20 flex-shrink-0 cursor-default">
                                        {{ sale.customerName ? sale.customerName.charAt(0) : '?' }}
                                    </div>
                                    <span 
                                        @mouseenter="showTooltip($event, sale)"
                                        @mouseleave="hideTooltip"
                                        class="text-white font-medium text-base cursor-help border-b border-dashed border-gray-600 hover:border-white transition-colors block truncate max-w-[180px]"
                                    >
                                        {{ sale.customerName || 'Silinmiş' }}
                                    </span>
                                </div>
                            </td>
                            
                            <td class="p-6 align-top">
                                <div v-if="sale.products && sale.products.length > 0" class="text-sm">
                                    
                                    <div v-if="sale.products.length <= 2">
                                        <div v-for="(prod, idx) in sale.products" :key="idx" class="flex items-center gap-2 mb-1.5 last:mb-0">
                                            <span class="w-1.5 h-1.5 rounded-full bg-indigo-500 flex-shrink-0"></span>
                                            <div class="flex items-center gap-2 w-full">
                                                <span class="text-gray-300 truncate max-w-[180px]" :title="prod.name">{{ prod.name }}</span>
                                                <span class="text-gray-500 text-xs font-mono bg-gray-800 px-1 rounded flex-shrink-0">x{{ prod.quantity }}</span>
                                            </div>
                                        </div>
                                    </div>

                                    <div v-else>
                                        <div v-for="(prod, idx) in sale.products.slice(0, 2)" :key="idx" class="flex items-center gap-2 mb-1.5">
                                            <span class="w-1.5 h-1.5 rounded-full bg-indigo-500 flex-shrink-0"></span>
                                            <div class="flex items-center gap-2 w-full">
                                                <span class="text-gray-300 truncate max-w-[180px]" :title="prod.name">{{ prod.name }}</span>
                                                <span class="text-gray-500 text-xs font-mono bg-gray-800 px-1 rounded flex-shrink-0">x{{ prod.quantity }}</span>
                                            </div>
                                        </div>

                                        <div v-if="expandedRows[sale.id]" class="animate-expand">
                                            <div v-for="(prod, idx) in sale.products.slice(2)" :key="idx" class="flex items-center gap-2 mb-1.5">
                                                <span class="w-1.5 h-1.5 rounded-full bg-gray-600 flex-shrink-0"></span>
                                                <div class="flex items-center gap-2 w-full">
                                                    <span class="text-gray-400 truncate max-w-[180px]" :title="prod.name">{{ prod.name }}</span>
                                                    <span class="text-gray-600 text-xs font-mono bg-gray-800/50 px-1 rounded flex-shrink-0">x{{ prod.quantity }}</span>
                                                </div>
                                            </div>
                                        </div>

                                        <button 
                                            @click.stop="toggleRowExpansion(sale.id)"
                                            class="text-[10px] font-bold uppercase tracking-wider mt-1 px-2 py-1 rounded transition-all flex items-center gap-1 select-none"
                                            :class="expandedRows[sale.id] ? 'bg-gray-700 text-gray-400 hover:bg-gray-600' : 'bg-indigo-500/20 text-indigo-400 hover:bg-indigo-500/30'"
                                        >
                                            <span v-if="expandedRows[sale.id]">- Daha Az</span>
                                            <span v-else>+{{ sale.products.length - 2 }} Diğer</span>
                                        </button>
                                    </div>

                                </div>
                                <span v-else class="text-gray-600 text-xs italic">Ürün yok</span>
                            </td>

                            <td class="p-6 text-gray-400 text-sm align-top">{{ formatDate(sale.saleDate || '') }}</td>
                            
                            <td class="p-6 text-right align-top">
                                <div class="text-white font-bold font-mono text-base">{{ formatCurrency(sale.totalAmount || 0) }}</div>
                            </td>
                            
                            <td class="p-6 text-right align-top">
                                <div class="text-emerald-400 font-bold font-mono text-base">
                                    {{ formatCurrency(calculateNetProfit(sale)) }}
                                </div>
                                <div v-if="calculateTotalDeductions(sale) > 0" class="text-[10px] text-gray-500 mt-1">
                                    -{{ formatCurrency(calculateTotalDeductions(sale)) }} (Gider)
                                </div>
                            </td>

                            <td class="p-6 text-center align-top">
                                <span :class="{
                                    'px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide': true,
                                    'bg-green-500/10 text-green-400': sale.paymentStatus === 'Ödendi' || sale.paymentStatus === 'Tamamlandı',
                                    'bg-yellow-500/10 text-yellow-400': sale.paymentStatus === 'Kısmi',
                                    'bg-red-500/10 text-red-400': sale.paymentStatus === 'Bekliyor'
                                }">
                                    {{ sale.paymentStatus || '-' }}
                                </span>
                                <div v-if="(sale.remainingAmount || 0) > 0.01" class="mt-2">
                                    <div class="text-[10px] text-red-400 opacity-90 font-bold uppercase">Kalan</div> 
                                    <div class="text-red-400 font-bold text-sm font-mono tracking-tight"> 
                                        {{ formatCurrency(sale.remainingAmount || 0) }}
                                    </div>
                                </div>
                            </td>

                            <td v-if="settingsStore.showShippingColumn" class="p-6 align-top">
                                <div class="relative">
                                    <select 
                                        :value="sale.shippingStatus || 'Sipariş Alındı'"
                                        @change="onStatusChange(sale, ($event.target as HTMLSelectElement).value)"
                                        class="bg-[#151521] border border-gray-700 text-gray-300 text-xs font-medium rounded-lg block w-full p-2.5 min-w-[120px] outline-none focus:border-indigo-500 cursor-pointer hover:bg-gray-800 transition-colors appearance-none"
                                        :class="{
                                            'text-yellow-400 border-yellow-500/30': sale.shippingStatus === 'Hazırlanıyor',
                                            'text-blue-400 border-blue-500/30': sale.shippingStatus === 'Kargoya Verildi',
                                            'text-green-400 border-green-500/30': sale.shippingStatus === 'Teslim Edildi',
                                            'text-red-400 border-red-500/30': sale.shippingStatus === 'İade'
                                        }"
                                    >
                                        <option v-for="option in shippingOptions" :key="option.value" :value="option.value">
                                            {{ option.label }}
                                        </option>
                                    </select>
                                    <div class="absolute inset-y-0 right-0 flex items-center px-2 pointer-events-none text-gray-500">
                                        <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
                                    </div>
                                </div>
                            </td>
                            
                            <td class="p-6 text-right pr-8 align-top">
                                <div class="flex items-center justify-end gap-2">
                                    <button @click="emit('pay', sale)" :disabled="(sale.remainingAmount || 0) <= 0.01" :class="['p-2 rounded-lg transition-all border flex items-center justify-center', (sale.remainingAmount || 0) <= 0.01 ? 'bg-gray-700 text-gray-500 border-transparent cursor-not-allowed opacity-50' : 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20 hover:bg-emerald-500 hover:text-white hover:border-transparent']" title="Tahsilat Yap">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path d="M4 4a2 2 0 00-2 2v4a2 2 0 002 2V6h10a2 2 0 00-2-2H4zm2 6a2 2 0 012-2h8a2 2 0 012 2v4a2 2 0 01-2 2H8a2 2 0 01-2-2v-4zm6 4a1 1 0 100-2 1 1 0 000 2z" /></svg>
                                    </button>
                                    <button @click="emit('delete', sale.id)" class="p-2 rounded-lg transition-all border bg-red-500/10 text-red-400 border-red-500/20 hover:bg-red-500 hover:text-white hover:border-transparent flex items-center justify-center" title="Siparişi Sil">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 10-2 0h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" /></svg>
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div v-if="totalPages > 1" class="p-4 border-t border-gray-800 flex items-center justify-between bg-[#151521]">
                 <span class="text-sm text-gray-500">Toplam {{ totalCount }} kayıttan {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} arası gösteriliyor</span>
                 <div class="flex items-center space-x-2">
                     <button @click="emit('page-change', currentPage - 1)" :disabled="currentPage === 1" class="px-3 py-1 rounded bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-50 border border-gray-700">&lt;</button>
                     <div class="flex space-x-1">
                         <button v-for="p in totalPages" :key="p" @click="emit('page-change', p)" v-show="Math.abs(p - currentPage) < 3 || p === 1 || p === totalPages" :class="['px-3 py-1 rounded border text-sm font-medium', currentPage === p ? 'bg-indigo-600 text-white border-indigo-600' : 'bg-gray-800 text-gray-300 border-gray-700 hover:bg-gray-700']">{{ p }}</button>
                     </div>
                     <button @click="emit('page-change', currentPage + 1)" :disabled="currentPage === totalPages" class="px-3 py-1 rounded bg-gray-800 text-gray-300 hover:bg-gray-700 disabled:opacity-50 border border-gray-700">&gt;</button>
                 </div>
            </div>
        </div>
        
        <div v-if="!isLoading && sales.length === 0" class="p-10 text-center text-gray-500 text-lg">Kayıt bulunamadı.</div>

        <Teleport to="body">
            <div 
                v-if="tooltip.visible && tooltip.data"
                class="fixed z-[9999] w-72 bg-[#1E1E2D] border border-gray-600 rounded-xl shadow-[0_10px_40px_rgba(0,0,0,0.6)] p-4 pointer-events-none transform -translate-x-1/2"
                :style="{ top: tooltip.y + 'px', left: tooltip.x + 'px' }"
                :class="{ '-translate-y-full': tooltip.placement === 'top' }"
            >
                <div 
                    class="absolute w-4 h-4 bg-[#1E1E2D] border-gray-600 transform rotate-45 left-1/2 -translate-x-1/2"
                    :class="tooltip.placement === 'top' ? '-bottom-2 border-b border-r' : '-top-2 border-t border-l'"
                ></div>

                <div class="space-y-3 relative z-10">
                    <h4 class="text-white font-bold border-b border-gray-700 pb-2 mb-2 flex items-center gap-2 text-sm">
                        <span>👤</span> Müşteri Bilgileri
                    </h4>
                    
                    <div class="flex items-start gap-3">
                        <span class="text-lg">📞</span>
                        <div>
                            <span class="block text-[10px] text-gray-500 font-bold uppercase tracking-wider">Telefon</span>
                            <span class="text-gray-300 text-sm font-mono">{{ tooltip.data.customerPhone || '-' }}</span>
                        </div>
                    </div>

                    <div class="flex items-start gap-3">
                        <span class="text-lg">📧</span>
                        <div>
                            <span class="block text-[10px] text-gray-500 font-bold uppercase tracking-wider">Email</span>
                            <span class="text-gray-300 text-sm break-all">{{ tooltip.data.customerEmail || '-' }}</span>
                        </div>
                    </div>

                    <div class="flex items-start gap-3">
                        <span class="text-lg">📍</span>
                        <div>
                            <span class="block text-[10px] text-gray-500 font-bold uppercase tracking-wider">Adres</span>
                            <span class="text-gray-300 text-sm leading-snug">{{ tooltip.data.customerAddress || '-' }}</span>
                        </div>
                    </div>
                </div>
            </div>
        </Teleport>

    </div>
</template>

<style scoped>
@keyframes glow-pulse {
    0% { background-color: rgba(99, 102, 241, 0.3); border-color: #6366f1; transform: scale(1.02); box-shadow: 0 0 20px rgba(99, 102, 241, 0.4); }
    50% { background-color: rgba(99, 102, 241, 0.15); border-color: #818cf8; transform: scale(1.01); box-shadow: 0 0 10px rgba(99, 102, 241, 0.2); }
    100% { background-color: transparent; border-color: transparent; transform: scale(1); box-shadow: none; }
}
.highlight-glow { animation: glow-pulse 4s ease-out forwards; border-left-width: 4px; border-left-color: #6366f1; }

@keyframes expand {
    from { opacity: 0; transform: translateY(-5px); max-height: 0; }
    to { opacity: 1; transform: translateY(0); max-height: 500px; }
}
.animate-expand { animation: expand 0.3s ease-out forwards; }
</style>