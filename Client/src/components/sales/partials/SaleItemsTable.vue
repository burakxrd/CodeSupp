<script setup lang="ts">
import { computed } from 'vue';

// --- TYPES ---
interface Product {
    id: number;
    name: string;
    code?: string;
    stock: number;
    price?: number;
    [key: string]: any;
}

interface SaleItem {
    productId: number | string;
    quantity: number;
    unitPrice: number;
    searchQuery?: string;     // [DÜZELTME] ? ekleyerek opsiyonel yaptık
    showDropdown?: boolean;   // [DÜZELTME] ? ekleyerek opsiyonel yaptık
}

interface Props {
    modelValue: SaleItem[];
    products: Product[];
}

// --- PROPS & EMITS ---
const props = withDefaults(defineProps<Props>(), {
    modelValue: () => [],
    products: () => []
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: SaleItem[]): void;
}>();

// --- HELPER FUNCTIONS ---
const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
};

const getProductStock = (productId: number | string) => {
    const p = props.products.find(x => x.id === productId);
    return p ? p.stock : 0;
};

// --- ARAMA VE SEÇİM ---
const getFilteredProducts = (query: string) => {
    if (!query) return props.products;
    const lowerQ = query.toLowerCase();
    return props.products.filter(p => 
        p.name.toLowerCase().includes(lowerQ) || 
        (p.code && p.code.toLowerCase().includes(lowerQ))
    );
};

const onProductSearch = (item: SaleItem) => {
    item.productId = ''; 
    item.unitPrice = 0;
    item.showDropdown = true;
};

const onInputBlur = (item: SaleItem) => {
    // Dropdown'ın kapanmasını biraz geciktirmek (click event'ini yakalamak için)
    // bazen gerekebilir ama şimdilik isteğin üzerine direkt kapatıyoruz.
    // Eğer tıklama sorunu yaşarsan buraya küçük bir setTimeout ekleyebilirsin.
    // item.showDropdown = false; 
    
    // Not: @mousedown.prevent kullandığımız için selectProduct çalışır,
    // ancak blur hemen tetiklenirse sorun olabilir. 
    // Mevcut JS kodunda setTimeout kaldırıldığı için burada da eklemedim.
    // Ancak blur event'i click'ten önce çalışırsa seçim yapılamaz.
    // Genellikle dropdown içindeki elemanlara @mousedown.prevent verilir (aşağıda verilmiş).
    
    // Güvenlik için küçük bir delay (opsiyonel, orijinal kodda kaldırıldı denmişti ama UX için önerilir)
    // setTimeout(() => { item.showDropdown = false; }, 200);
    
    // Orijinal koda sadık kalıyoruz:
    item.showDropdown = false;
};

const selectProduct = (item: SaleItem, product: Product) => {
    item.productId = product.id;
    item.searchQuery = product.code ? `[${product.code}] ${product.name}` : product.name;
    item.unitPrice = product.price || 0;
    item.showDropdown = false;
};

// --- SATIR YÖNETİMİ ---
const addItemRow = () => { 
    const newItems: SaleItem[] = [...props.modelValue, { 
        productId: '', quantity: 1, unitPrice: 0, searchQuery: '', showDropdown: false 
    }];
    emit('update:modelValue', newItems);
};

const removeItemRow = (index: number) => {
    const newItems = [...props.modelValue];
    newItems.splice(index, 1);
    emit('update:modelValue', newItems);
};

// --- HESAPLAMALAR ---
const itemsTotal = computed(() => {
    return props.modelValue.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0);
});
</script>

<template>
    <div class="flex flex-col h-full overflow-hidden">
        
        <div class="hidden md:grid grid-cols-12 gap-4 text-gray-500 text-xs uppercase font-bold tracking-wider mb-2 px-1 flex-shrink-0">
            <div class="col-span-5 pl-1">Ürün (Kod veya İsim)</div>
            <div class="col-span-2 text-center">Adet</div>
            <div class="col-span-2 text-center">Birim Fiyat</div>
            <div class="col-span-2 text-right pr-2">Toplam</div>
            <div class="col-span-1"></div>
        </div>

        <div class="space-y-4 overflow-y-auto pr-2 custom-scrollbar flex-1 min-h-0">
            <div v-for="(item, index) in modelValue" :key="index" class="bg-[#151521]/50 p-2 rounded-xl flex flex-col md:grid md:grid-cols-12 gap-4 items-start relative group transition-colors">
                
                <div class="col-span-5 w-full relative">
                    <input 
                        type="text"
                        v-model="item.searchQuery"
                        @input="onProductSearch(item)"
                        @focus="item.showDropdown = true"
                        @blur="onInputBlur(item)"
                        placeholder="Ürün ara (Kod: TS1...)"
                        class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 text-white text-sm focus:border-[#3699FF] outline-none h-11"
                        :class="{'border-red-500': !item.productId && item.searchQuery && !item.showDropdown}"
                        required
                    />
                    
                    <div v-if="item.productId" class="absolute right-3 top-1/2 -translate-y-1/2 text-green-400">✓</div>

                    <div v-if="item.showDropdown" class="absolute left-0 right-0 top-full mt-1 bg-[#1E1E2D] border border-gray-600 rounded-lg shadow-xl z-50 max-h-72 overflow-y-auto custom-scrollbar">
                        <div 
                            v-for="prod in getFilteredProducts(item.searchQuery)" 
                            :key="prod.id"
                            @mousedown.prevent="selectProduct(item, prod)"
                            class="px-3 py-2 hover:bg-[#3699FF]/20 cursor-pointer border-b border-gray-700/50 last:border-0 flex justify-between items-center"
                        >
                            <div>
                                <span v-if="prod.code" class="text-yellow-400 font-mono mr-2 text-xs">[{{ prod.code }}]</span>
                                <span class="text-white text-sm">{{ prod.name }}</span>
                            </div>
                            <span class="text-gray-400 text-xs">Stok: {{ prod.stock }}</span>
                        </div>
                        <div v-if="getFilteredProducts(item.searchQuery).length === 0" class="p-3 text-gray-500 text-sm text-center">
                            Sonuç bulunamadı.
                        </div>
                    </div>

                    <div v-if="item.productId" class="mt-2 flex items-center gap-2">
                        <span class="text-xs text-gray-400">Stok:</span>
                        <span :class="['px-2 py-0.5 rounded text-xs font-bold transition-colors', getProductStock(item.productId) < item.quantity ? 'bg-orange-500/20 text-orange-400' : 'bg-green-500/20 text-green-400']">
                            {{ getProductStock(item.productId) }}
                        </span>
                        
                        <span v-if="getProductStock(item.productId) < item.quantity" class="text-xs text-orange-400 font-medium flex items-center gap-1 animate-pulse">
                            ⚠️ Eksiye Düşecek
                        </span>
                    </div>
                </div>

                <div class="col-span-2 w-full">
                    <input v-model.number="item.quantity" type="number" min="1" class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 text-white text-sm text-center focus:border-[#3699FF] outline-none h-11 font-mono" required>
                </div>

                <div class="col-span-2 w-full">
                    <div class="relative">
                        <input v-model.number="item.unitPrice" type="number" step="0.01" class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 text-white text-sm text-right focus:border-[#3699FF] outline-none h-11 font-mono" required>
                        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-xs">₺</span>
                    </div>
                </div>

                <div class="col-span-2 w-full flex items-center justify-end h-11">
                    <span class="text-white font-mono font-medium text-base">{{ formatCurrency(item.quantity * item.unitPrice) }}</span>
                </div>

                <div class="col-span-1 w-full flex items-center justify-center h-11">
                    <button type="button" @click="removeItemRow(index)" class="text-red-500 hover:text-red-400 hover:bg-red-500/10 p-2 rounded-lg transition-colors" v-if="modelValue.length > 1">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 10-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
                        </svg>
                    </button>
                </div>
            </div>
        </div>

        <div class="mt-2 flex-shrink-0">
            <button type="button" @click="addItemRow" class="text-[#3699FF] hover:text-[#0073E9] text-sm font-bold flex items-center gap-2 transition-colors py-2">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M10 5a1 1 0 011 1v3h3a1 1 0 110 2h-3v3a1 1 0 11-2 0v-3H6a1 1 0 11-2 0v-3H6a1 1 0 110-2h3V6a1 1 0 011-1z" clip-rule="evenodd" />
                </svg>
                Başka Ürün Ekle
            </button>
        </div>

        <div class="flex-shrink-0 flex justify-between items-center border-t border-gray-700 pt-4 mt-auto">
            <span class="text-gray-400 font-medium">Ürünler Toplamı:</span>
            <span class="text-white font-bold text-xl font-mono">
                {{ formatCurrency(itemsTotal) }}
            </span>
        </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #151521; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #374151; border-radius: 3px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #4B5563; }
</style>