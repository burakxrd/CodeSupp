<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import api from '../../services/api';
import { formatCurrency, formatDate } from '../../utils/formatters';
import type { Customer, Sale } from '../../types'; 

// --- TYPES ---

// CustomerDetail: Müşteri kartı için genişletilmiş tip
interface CustomerDetail extends Customer {
    orderCount?: number;
    totalSpent?: number;
    isVip?: boolean;
    isLoyal?: boolean;
    isRisky?: boolean;
    [key: string]: any;
}

// DÜZELTME BURADA: Sale tipini genişletiyoruz (SaleRow)
// TypeScript'e "Bu objenin içinde orderCode veya saleDate olabilir" diyoruz.
interface SaleRow extends Sale {
    orderCode?: string;
    saleDate?: string; // Ana tipte 'date' var ama bazen 'saleDate' gelebilir
}

interface Props {
    isOpen: boolean;
    customer?: CustomerDetail | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    customer: null
});

const emit = defineEmits<{
    (e: 'close'): void;
}>();

// --- LOCAL STATE ---
// DÜZELTME: ref tipini Sale yerine SaleRow yapıyoruz
const sales = ref<SaleRow[]>([]);
const isLoadingSales = ref<boolean>(false);
const currentPage = ref<number>(1);
const totalCount = ref<number>(0);
const pageSize = 5; 

// --- DATA FETCHING ---
const fetchCustomerSales = async () => {
    if (!props.customer?.id) return;
    
    isLoadingSales.value = true;
    try {
        const res: any = await api.getSales(
            currentPage.value,
            pageSize,
            undefined, 
            undefined, 
            undefined, 
            undefined, 
            undefined, 
            'date',    
            'desc',    
            props.customer.id 
        );
        
        // API'den gelen items dizisini SaleRow[] olarak atıyoruz
        sales.value = res.items || [];
        totalCount.value = res.totalCount || 0;
    } catch (err) {
        console.error("Sipariş geçmişi yüklenemedi:", err);
    } finally {
        isLoadingSales.value = false;
    }
};

watch(
    () => props.isOpen, 
    (newVal) => {
        if (newVal && props.customer) {
            currentPage.value = 1;
            fetchCustomerSales();
        }
    }
);

const totalPages = computed(() => Math.ceil(totalCount.value / pageSize));

const changePage = (p: number) => {
    if (p < 1 || p > totalPages.value) return;
    currentPage.value = p;
    fetchCustomerSales();
};
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div @click="$emit('close')" class="absolute inset-0 bg-black/80 backdrop-blur-sm transition-opacity"></div>

        <div class="relative bg-[#1E1E2D] rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-hidden flex flex-col border border-gray-700 animate-scale-in">
            
            <div class="flex items-center justify-between p-6 border-b border-gray-700 bg-[#151521]">
                <h2 class="text-xl font-bold text-white flex items-center gap-3">
                    <span class="text-2xl">👤</span> Müşteri Profili
                </h2>
                <button @click="$emit('close')" class="p-2 hover:bg-white/10 rounded-lg text-gray-400 hover:text-white transition-colors">
                    ✕
                </button>
            </div>

            <div class="overflow-y-auto custom-scrollbar p-6 space-y-8">
                
                <div v-if="customer" class="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div class="col-span-1 bg-[#2B2B40] rounded-xl p-6 text-center border border-gray-700 relative overflow-hidden group">
                        <div class="absolute inset-0 bg-gradient-to-b from-transparent to-black/20 pointer-events-none"></div>
                        <div class="w-24 h-24 mx-auto rounded-full bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-4xl font-bold text-white shadow-xl mb-4">
                            {{ customer.name ? customer.name.charAt(0) : '?' }}
                        </div>
                        <h3 class="text-xl font-bold text-white mb-1">{{ customer.name }}</h3>
                        <div class="flex flex-wrap justify-center gap-2 mt-3">
                            <span v-if="customer.isVip" class="px-2 py-1 rounded text-xs font-bold bg-purple-900 text-purple-200 border border-purple-700">VIP</span>
                            <span v-if="customer.isLoyal" class="px-2 py-1 rounded text-xs font-bold bg-blue-900 text-blue-200 border border-blue-700">SADIK</span>
                            <span v-if="customer.isRisky" class="px-2 py-1 rounded text-xs font-bold bg-orange-900 text-orange-200 border border-orange-700">RİSKLİ</span>
                        </div>
                    </div>

                    <div class="col-span-1 md:col-span-2 bg-[#2B2B40] rounded-xl p-6 border border-gray-700 flex flex-col justify-center">
                        <div class="space-y-4">
                            <div class="flex items-center gap-3 text-gray-300">
                                <span class="bg-gray-700 p-2 rounded-lg">📧</span>
                                <span class="font-mono">{{ customer.email || 'E-posta yok' }}</span>
                            </div>
                            <div class="flex items-center gap-3 text-gray-300">
                                <span class="bg-gray-700 p-2 rounded-lg">📞</span>
                                <span class="font-mono">{{ customer.phone || 'Telefon yok' }}</span>
                            </div>
                            <div class="flex items-center gap-3 text-gray-300">
                                <span class="bg-gray-700 p-2 rounded-lg">📸</span>
                                <a v-if="customer.instagramHandle" :href="`https://instagram.com/${customer.instagramHandle.replace('@','')}`" target="_blank" class="text-indigo-400 hover:text-indigo-300 hover:underline">
                                    {{ customer.instagramHandle }}
                                </a>
                                <span v-else class="text-gray-500">-</span>
                            </div>
                            <div class="flex items-start gap-3 text-gray-300">
                                <span class="bg-gray-700 p-2 rounded-lg mt-1">📍</span>
                                <span class="flex-1">{{ customer.address || 'Adres girilmemiş.' }}</span>
                            </div>
                        </div>
                    </div>
                </div>

                <div v-if="customer" class="grid grid-cols-2 gap-4">
                    <div class="bg-[#151521] p-4 rounded-xl border border-gray-800 flex items-center justify-between">
                        <div>
                            <div class="text-gray-500 text-sm mb-1">Toplam Sipariş</div>
                            <div class="text-2xl font-bold text-white">{{ customer.orderCount || 0 }} Adet</div>
                        </div>
                        <div class="text-3xl opacity-20">📦</div>
                    </div>
                    <div class="bg-[#151521] p-4 rounded-xl border border-gray-800 flex items-center justify-between">
                        <div>
                            <div class="text-gray-500 text-sm mb-1">Toplam Harcama</div>
                            <div class="text-2xl font-bold text-green-400">{{ formatCurrency(customer.totalSpent || 0) }}</div>
                        </div>
                        <div class="text-3xl opacity-20">💰</div>
                    </div>
                </div>

                <div>
                    <h4 class="text-lg font-bold text-white mb-4 flex items-center gap-2">
                        <span>🕒</span> Sipariş Geçmişi
                    </h4>

                    <div class="bg-[#151521] rounded-xl border border-gray-800 overflow-hidden">
                        <table class="w-full text-left border-collapse">
                            <thead>
                                <tr class="bg-gray-800/50 text-gray-400 text-xs uppercase font-bold">
                                    <th class="p-4">Kod</th>
                                    <th class="p-4">Tarih</th>
                                    <th class="p-4 text-right">Tutar</th>
                                    <th class="p-4 text-center">Durum</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-800">
                                <tr v-if="isLoadingSales">
                                    <td colspan="4" class="p-8 text-center text-gray-500 animate-pulse">Yükleniyor...</td>
                                </tr>
                                <tr v-else-if="sales.length === 0">
                                    <td colspan="4" class="p-8 text-center text-gray-500">Henüz sipariş yok.</td>
                                </tr>
                                <tr v-for="sale in sales" :key="sale.id" class="hover:bg-white/5 transition-colors">
                                    <td class="p-4 text-white font-mono text-sm">
                                        #{{ sale.orderCode || sale.id }}
                                    </td>
                                    <td class="p-4 text-gray-400 text-sm">
                                        {{ formatDate(sale.date || sale.saleDate) }}
                                    </td>
                                    <td class="p-4 text-right text-green-400 font-medium">{{ formatCurrency(sale.totalAmount) }}</td>
                                    <td class="p-4 text-center">
                                        <span class="px-2 py-1 rounded text-[10px] font-bold uppercase bg-gray-700 text-gray-300">
                                            {{ sale.shippingStatus || 'Belirsiz' }}
                                        </span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>

                        <div v-if="totalPages > 1" class="p-3 border-t border-gray-800 flex items-center justify-between bg-[#1E1E2D]">
                            <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="px-3 py-1 rounded bg-gray-700 hover:bg-gray-600 disabled:opacity-50 text-xs">Önceki</button>
                            <span class="text-xs text-gray-400">{{ currentPage }} / {{ totalPages }}</span>
                            <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="px-3 py-1 rounded bg-gray-700 hover:bg-gray-600 disabled:opacity-50 text-xs">Sonraki</button>
                        </div>
                    </div>
                </div>

            </div>

        </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #1E1E2D; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #444; border-radius: 3px; }
.animate-scale-in { animation: scaleIn 0.2s ease-out; }
@keyframes scaleIn { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
</style>