<script setup lang="ts">
import BaseButton from '../../components/ui/BaseButton.vue';
import { getImageUrl } from '../../utils/urlHelper'; 
import { formatCurrency, formatDateTime } from '../../utils/formatters'; 
import type { Purchase } from '../../types'; 

// --- PROPS ---
interface Props {
    purchases: Purchase[];
    isLoading?: boolean;
    totalCount?: number;
    currentPage?: number;
    totalPages?: number;
    pageSize?: number;
}

const props = withDefaults(defineProps<Props>(), {
    purchases: () => [],
    isLoading: false,
    totalCount: 0,
    currentPage: 1,
    totalPages: 1,
    pageSize: 20
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'page-change', page: number): void;
    (e: 'edit', item: Purchase): void;
    (e: 'delete', id: number): void;
}>();

</script>

<template>
    <div class="bg-[#1E1E2D] rounded-xl shadow-xl border border-gray-700/50 overflow-hidden flex flex-col relative min-h-[400px]">
        
        <div v-if="isLoading" class="absolute inset-0 z-10 bg-[#1E1E2D]/80 backdrop-blur-sm flex items-center justify-center">
            <div class="flex flex-col items-center gap-3">
                <svg class="animate-spin h-8 w-8 text-indigo-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <span class="text-gray-400 text-sm font-medium">Veriler yükleniyor...</span>
            </div>
        </div>

        <div class="overflow-x-auto flex-1">
            <table class="w-full text-left border-collapse min-w-[1000px]">
                
                <thead class="sticky top-0 z-0 bg-[#151521] shadow-sm">
                    <tr class="text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-700">
                        <th class="p-5 pl-8">Tarih</th>
                        <th class="p-5">Ürün</th>
                        <th class="p-5">Adet</th>
                        <th class="p-5">Birim Fiyat</th>
                        <th class="p-5">Kargo/Kg</th>
                        <th class="p-5">Toplam Maliyet</th>
                        <th class="p-5 text-right pr-8">İşlemler</th>
                    </tr>
                </thead>

                <tbody class="divide-y divide-gray-800/50">
                    
                    <tr v-if="!isLoading && purchases.length === 0">
                        <td colspan="7" class="p-12 text-center">
                            <div class="flex flex-col items-center justify-center text-gray-500">
                                <span class="text-4xl mb-3">📭</span>
                                <h3 class="text-lg font-medium text-gray-400">Kayıt Bulunamadı</h3>
                                <p class="text-sm">Henüz bir alım kaydı eklenmemiş.</p>
                            </div>
                        </td>
                    </tr>

                    <tr v-for="item in purchases" :key="item.id" class="group hover:bg-[#232333] transition-colors duration-200">
                        
                        <td class="p-5 pl-8">
                            <div class="text-gray-300 text-sm font-medium">{{ formatDateTime(item.purchaseDate) }}</div>
                        </td>
                        
                        <td class="p-5">
                            <div class="flex items-center">
                                <div v-if="item.product && item.product.imagePath" class="w-10 h-10 rounded-lg overflow-hidden border border-gray-700/50 mr-3 shrink-0 group-hover:border-indigo-500/50 transition-colors">
                                    <img 
                                        :src="getImageUrl(item.product.imagePath)"
                                        alt="Product"  
                                        class="w-full h-full object-cover"
                                        @error="($event.target as HTMLImageElement).style.display='none'" 
                                    />
                                </div>
                                
                                <span v-else class="bg-indigo-500/10 text-indigo-400 p-2 rounded-lg mr-3 shrink-0 w-10 h-10 flex items-center justify-center border border-indigo-500/10">
                                    📦
                                </span>

                                <div>
                                    <div class="text-white font-bold text-sm">
                                        {{ item.product ? item.product.name : 'Silinmiş Ürün' }}
                                    </div>
                                    <div v-if="item.product && item.product.code" class="text-xs text-gray-500 font-mono mt-0.5">
                                        {{ item.product.code }}
                                    </div>
                                </div>
                            </div>
                        </td>
                        
                        <td class="p-5">
                            <span class="bg-gray-700/30 text-gray-300 px-2.5 py-1 rounded-md text-xs font-bold border border-gray-600/50">
                                {{ item.quantityInUnits }} Adet
                            </span>
                        </td>
                        
                        <td class="p-5">
                            <span class="text-gray-400 font-mono text-sm">{{ formatCurrency(item.productPricePerUnit) }}</span>
                        </td>

                        <td class="p-5">
                            <span class="text-gray-400 font-mono text-sm">{{ formatCurrency(item.shippingCostPerKg) }}</span>
                        </td>
                        
                        <td class="p-5">
                            <div class="flex flex-col">
                                <span class="text-emerald-400 font-bold font-mono text-base tracking-tight">
                                    {{ formatCurrency(item.totalCost) }}
                                </span>
                            </div>
                        </td>
                        
                        <td class="p-5 text-right pr-8">
                            <div class="flex items-center justify-end gap-2 opacity-80 group-hover:opacity-100 transition-opacity">
                                <BaseButton 
                                    variant="secondary" 
                                    class="!px-3 !py-1.5 !text-xs !h-8"
                                    @click="emit('edit', item)"
                                >
                                    ✏️
                                </BaseButton>

                                <BaseButton 
                                    variant="danger" 
                                    class="!px-3 !py-1.5 !text-xs !h-8"
                                    @click="emit('delete', item.id)"
                                >
                                    🗑️
                                </BaseButton>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div v-if="totalPages > 1" class="p-4 border-t border-gray-700/50 flex items-center justify-between bg-[#151521]">
            <span class="text-xs text-gray-500">
                Toplam {{ totalCount }} kayıttan {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount || 0) }} arası
            </span>
            
            <div class="flex items-center gap-1">
                <button 
                    @click="emit('page-change', currentPage - 1)" 
                    :disabled="currentPage === 1"
                    class="w-8 h-8 flex items-center justify-center rounded hover:bg-white/5 text-gray-400 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                    &lt;
                </button>
                
                <button 
                    v-for="p in totalPages" 
                    :key="p" 
                    @click="emit('page-change', p)"
                    v-show="Math.abs(p - currentPage) < 3 || p === 1 || p === totalPages"
                    class="w-8 h-8 flex items-center justify-center rounded-lg text-xs font-bold transition-all border"
                    :class="currentPage === p 
                        ? 'bg-indigo-600 text-white border-indigo-600 shadow-lg shadow-indigo-500/20' 
                        : 'bg-transparent text-gray-400 border-transparent hover:bg-white/5 hover:text-white'"
                >
                    {{ p }}
                </button>

                <button 
                    @click="emit('page-change', currentPage + 1)" 
                    :disabled="currentPage === totalPages"
                    class="w-8 h-8 flex items-center justify-center rounded hover:bg-white/5 text-gray-400 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                    &gt;
                </button>
            </div>
        </div>

    </div>
</template>