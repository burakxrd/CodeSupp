<script setup lang="ts">
import { getImageUrl } from '../../utils/urlHelper';
import type { Product } from '../../types'; 

// --- PROPS ---
interface Props {
    products: Product[];
    isLoading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    products: () => [],
    isLoading: false
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'edit', product: Product): void;
    (e: 'delete', id: number): void;
    (e: 'preview-image', path: string): void;
}>();

// --- HELPERS ---

const formatCurrency = (value?: number): string => {
    if (value === null || value === undefined) return '₺0,00';
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
};

// İndirim Oranı Hesaplama
const calculateDiscountRate = (price?: number, discountedPrice?: number): number => {
    if (!price || !discountedPrice || discountedPrice >= price) return 0;
    return Math.round(((price - discountedPrice) / price) * 100);
};

// Kâr ve Marj Hesaplama
const getProfitInfo = (product: Product) => {
    const cost = product.averageUnitCostInTRY || 0;
    // Eğer indirim varsa, kârı indirimli fiyat üzerinden hesapla, yoksa normal fiyat
    const finalPrice = (product.discountedPrice && product.discountedPrice < product.price) 
        ? product.discountedPrice 
        : product.price;
        
    const profit = finalPrice - cost;
    // Sıfıra bölme hatasını önle
    const margin = finalPrice > 0 ? (profit / finalPrice) * 100 : 0;
    
    return {
        amount: profit,
        margin: Math.round(margin),
        isPositive: profit >= 0
    };
};

const getTypeBadgeClass = (typeName?: string): string => {
    switch (typeName) {
        case 'Dijital': return 'bg-purple-500/10 text-purple-400 border border-purple-500/20';
        case 'Hizmet': return 'bg-orange-500/10 text-orange-400 border border-orange-500/20';
        default: return 'bg-blue-500/10 text-blue-400 border border-blue-500/20';
    }
};

const getTypeIcon = (typeName?: string): string => {
    switch (typeName) {
        case 'Dijital': return '💻';
        case 'Hizmet': return '🛠️';
        default: return '📦';
    }
};
</script>

<template>
    <div class="overflow-x-auto min-h-[400px]">
        <table class="w-full text-left border-collapse min-w-[1100px]">
            <thead>
                <tr class="bg-[#151521] text-gray-400 text-sm font-bold uppercase tracking-wider border-b border-gray-800">
                    <th class="p-6 pl-8">Kod & Tip</th> 
                    <th class="p-6">Ürün Detayı</th> 
                    <th class="p-6">Kategori</th>
                    <th class="p-6">Açıklama</th>
                    <th class="p-6">Maliyet / Kâr</th> <th class="p-6">Satış Fiyatı</th> 
                    <th class="p-6">Stok & Satış</th> <th class="p-6 text-right pr-8">İşlemler</th>
                </tr>
            </thead>

            <tbody v-if="isLoading" class="divide-y divide-gray-800">
                <tr>
                    <td colspan="8" class="p-6 text-center text-gray-500">Yükleniyor...</td> 
                </tr>
            </tbody>

            <tbody v-else class="divide-y divide-gray-800">
                <tr v-for="(product, index) in products" :key="product.id" class="group hover:bg-[#2B2B40] transition-colors">
                    
                    <td class="p-6 pl-8 align-top">
                        <div class="flex flex-col items-start gap-1.5">
                            <span v-if="product.code" class="bg-gray-700 text-yellow-400 px-2 py-1 rounded text-xs font-mono font-bold">{{ product.code }}</span>
                            <span v-else class="text-gray-600 text-xs italic">-</span>

                            <span :class="['px-2 py-0.5 rounded text-[10px] font-medium flex items-center gap-1 w-fit whitespace-nowrap', getTypeBadgeClass(product.typeName)]">
                                <span>{{ getTypeIcon(product.typeName) }}</span>
                                {{ product.typeName || 'Fiziksel' }}
                            </span>
                        </div>
                    </td>
                    
                    <td class="p-6 align-top">
                        <div class="flex items-center">
                            <div 
                                v-if="product.imagePath"
                                @click="$emit('preview-image', product.imagePath!)"
                                class="h-12 w-12 rounded-lg overflow-hidden border border-gray-600 mr-4 shadow-sm cursor-zoom-in group-hover:border-indigo-500 transition-colors bg-black flex-shrink-0"
                            >
                                <img 
                                    :src="getImageUrl(product.imagePath)" 
                                    class="w-full h-full object-cover hover:scale-110 transition-transform duration-300" 
                                    alt="Ürün"
                                    @error="($event.target as HTMLImageElement).style.display='none'"
                                >
                            </div>

                            <div 
                                v-else 
                                class="h-12 w-12 rounded-lg bg-gray-700 text-gray-300 flex items-center justify-center text-lg font-bold mr-4 uppercase shadow-lg select-none flex-shrink-0"
                            >
                                {{ product.name ? product.name.charAt(0) : '?' }}
                            </div>

                            <div class="flex flex-col gap-1">
                                <span class="text-white font-medium text-base leading-none">{{ product.name }}</span>
                                
                                <div v-if="product.size || product.color" class="flex gap-2">
                                    <span v-if="product.size" class="text-[10px] bg-indigo-900/40 text-indigo-300 px-1.5 py-0.5 rounded border border-indigo-500/20">
                                        Beden: {{ product.size }}
                                    </span>
                                    <span v-if="product.color" class="text-[10px] bg-pink-900/40 text-pink-300 px-1.5 py-0.5 rounded border border-pink-500/20">
                                        Renk: {{ product.color }}
                                    </span>
                                </div>
                            </div>
                        </div>
                    </td>

                    <td class="p-6 align-top">
                        <span v-if="product.categoryName" class="bg-gray-700/50 text-gray-300 px-2.5 py-1 rounded-full text-xs font-medium border border-gray-600">
                            {{ product.categoryName }}
                        </span>
                        <span v-else class="text-gray-600 text-xs">-</span>
                    </td>

                    <td class="p-6 align-top">
                        <div class="relative group/desc cursor-help w-full max-w-[180px]">
                            <span v-if="product.description" class="text-xs text-gray-400 truncate block">
                                {{ product.description }}
                            </span>
                            <span v-else class="text-gray-600 text-xs italic">-</span>
                            
                            <div v-if="product.description" 
                                :class="[
                                    'absolute left-1/2 -translate-x-1/2 hidden group-hover/desc:block w-64 p-3 bg-gray-900 text-white text-xs rounded-lg shadow-xl border border-gray-700 z-50 whitespace-normal break-words pointer-events-none transition-opacity',
                                    index < 2 ? 'top-full mt-2' : 'bottom-full mb-2' 
                                ]"
                            >
                                {{ product.description }}
                                <div :class="[
                                    'absolute left-1/2 -translate-x-1/2 border-8 border-transparent',
                                    index < 2 ? 'bottom-full border-b-gray-900' : 'top-full border-t-gray-900'
                                ]"></div>
                            </div>
                        </div>
                    </td>

                    <td class="p-6 align-top">
                        <div class="flex flex-col">
                            <div class="text-red-400 font-bold font-mono text-lg leading-none">
                                {{ formatCurrency(product.averageUnitCostInTRY) }}
                            </div>
                            
                            <div class="mt-2 text-xs font-medium flex items-center gap-1"
                                 :class="getProfitInfo(product).isPositive ? 'text-green-500' : 'text-red-500'"
                            >
                                <span>{{ getProfitInfo(product).isPositive ? '↑' : '↓' }} Net:</span>
                                <span>{{ formatCurrency(getProfitInfo(product).amount) }}</span>
                                <span class="opacity-75">
                                    (%{{ getProfitInfo(product).margin }})
                                </span>
                            </div>
                        </div>
                    </td>

                    <td class="p-6 align-top">
                        <div class="flex flex-col items-start">
                            <div v-if="product.discountedPrice && product.discountedPrice < product.price">
                                <div class="flex items-center gap-2 mb-0.5">
                                    <span class="text-gray-500 line-through text-xs">{{ formatCurrency(product.price) }}</span>
                                    <span class="bg-red-500/20 text-red-400 text-[10px] font-bold px-1.5 py-0.5 rounded border border-red-500/30">
                                        %{{ calculateDiscountRate(product.price, product.discountedPrice) }}
                                    </span>
                                </div>
                                <span class="text-green-400 font-bold font-mono text-lg block">{{ formatCurrency(product.discountedPrice) }}</span>
                            </div>
                            
                            <div v-else class="text-green-400 font-bold font-mono text-lg">
                                {{ formatCurrency(product.price) }}
                            </div>

                            <span v-if="product.shippingType === 0" class="mt-1 text-[10px] bg-green-900/30 text-green-500 px-1.5 py-0.5 rounded border border-green-900/50 uppercase tracking-wide">
                                🚚 Ücretsiz
                            </span>
                        </div>
                    </td>

                    <td class="p-6 align-top">
                        <div class="flex flex-col items-start">
                            <span :class="['px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide whitespace-nowrap', product.stock < 10 ? 'bg-red-500/10 text-red-400' : 'bg-blue-500/10 text-blue-400']">
                                {{ product.stock }} Adet
                            </span>
                            
                            <div class="mt-2 text-[12px] text-gray-500 font-medium flex items-center gap-1">
                                <span>🛒 Toplam Satış:</span>
                                <span class="text-gray-300 font-bold">{{ product.totalSales || 0 }}</span>
                            </div>
                        </div>
                    </td>

                    <td class="p-6 text-right pr-8 align-top">
                        <div class="flex items-center justify-end space-x-2">
                            <button @click="$emit('edit', product)" class="bg-gray-700 hover:bg-blue-600 text-gray-300 hover:text-white p-2 rounded-lg transition-all" title="Düzenle">✏️</button>
                            <button @click="$emit('delete', product.id)" class="bg-gray-700 hover:bg-red-600 text-gray-300 hover:text-white p-2 rounded-lg transition-all" title="Sil">🗑️</button>
                        </div>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>