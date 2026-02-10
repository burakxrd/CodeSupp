<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import api from '../../services/api';
import BaseInput from '../../components/ui/BaseInput.vue'; 
import type { Product } from '../../types'; 

// --- CONSTANTS ---
const DEBOUNCE_MS = 300;
const PAGE_SIZE = 10;

// --- PROPS & EMITS ---
interface Props {
    modelValue?: string | number;
    initialName?: string;
    error?: string;
}

const props = withDefaults(defineProps<Props>(), {
    modelValue: '',
    initialName: '',
    error: ''
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: string | number): void;
    (e: 'update:name', value: string): void;
}>();

// --- STATE ---
const searchQuery = ref<string>('');
const showDropdown = ref<boolean>(false);
const productList = ref<Product[]>([]);
const isLoading = ref<boolean>(false);
const fetchError = ref<boolean>(false);
const focusedIndex = ref<number>(-1); 

// Timeout için doğru tip tanımı
let searchTimeout: ReturnType<typeof setTimeout> | null = null;

// --- WATCHERS ---
watch(() => props.initialName, (val) => {
    if (val) searchQuery.value = val;
}, { immediate: true });

// --- API ---
const fetchProducts = async () => {
    if (!searchQuery.value.trim()) return;
    
    isLoading.value = true;
    fetchError.value = false;
    
    try {
        // API yanıtının PaginatedResult<Product> olduğunu varsayıyoruz
        const res: any = await api.getProducts(1, PAGE_SIZE, searchQuery.value);
        productList.value = res.items || [];
    } catch (e) {
        console.error("Ürün arama hatası:", e);
        fetchError.value = true;
        productList.value = [];
    } finally {
        isLoading.value = false;
    }
};

// --- HANDLERS ---
const onInput = (event: Event | string) => {
    // BaseInput'tan gelen veri string olabilir veya native input event olabilir
    const val = typeof event === 'string' ? event : (event.target as HTMLInputElement).value;

    searchQuery.value = val;
    emit('update:modelValue', ''); // ID sıfırla (Kullanıcı değiştirdi)
    focusedIndex.value = -1; // Reset navigation
    
    if (searchTimeout) clearTimeout(searchTimeout);
    
    if (!val) {
        showDropdown.value = false;
        return;
    }

    showDropdown.value = true;
    // Debounce
    searchTimeout = setTimeout(fetchProducts, DEBOUNCE_MS);
};

const selectProduct = (prod: Product) => {
    if (!prod) return;
    searchQuery.value = prod.name;
    emit('update:modelValue', prod.id);
    emit('update:name', prod.name);
    showDropdown.value = false;
    focusedIndex.value = -1;
};

// --- KEYBOARD NAVIGATION (SENIOR TOUCH) ---
const onKeydown = (e: KeyboardEvent) => {
    if (!showDropdown.value || productList.value.length === 0) return;

    switch (e.key) {
        case 'ArrowDown':
            e.preventDefault();
            if (focusedIndex.value < productList.value.length - 1) focusedIndex.value++;
            scrollToFocused();
            break;
        case 'ArrowUp':
            e.preventDefault();
            if (focusedIndex.value > 0) focusedIndex.value--;
            scrollToFocused();
            break;
        case 'Enter':
            e.preventDefault();
            if (focusedIndex.value >= 0) selectProduct(productList.value[focusedIndex.value]);
            break;
        case 'Escape':
            showDropdown.value = false;
            break;
    }
};

// Scroll takibi (Klavye ile aşağı inerken scroll'u kaydırır)
const scrollToFocused = () => {
    nextTick(() => {
        // DOM elementini bul ve Cast et
        const activeItem = document.querySelector('.dropdown-item-active') as HTMLElement;
        if (activeItem) {
            activeItem.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }
    });
};

const onBlur = () => {
    // Tıklama event'i gerçekleşebilsin diye hafif gecikmeli kapatıyoruz
    setTimeout(() => { showDropdown.value = false; }, 200);
};
</script>

<template>
    <div class="relative w-full" @keydown="onKeydown">
        <BaseInput 
            label="Ürün Ara"
            v-model="searchQuery"
            @input="onInput($event)" 
            @focus="onInput(searchQuery)"
            @blur="onBlur"
            placeholder="Ürün adı yazın..."
            :error="error"
            required
            autocomplete="off"
        >
            <template #prepend>
                <span class="text-gray-500 transition-colors" :class="{'text-indigo-400': showDropdown}">🔍</span>
            </template>
            
            <template #append>
                <svg v-if="isLoading" class="animate-spin h-4 w-4 text-indigo-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
            </template>
        </BaseInput>

        <transition 
            enter-active-class="transition ease-out duration-100" 
            enter-from-class="transform opacity-0 scale-95" 
            enter-to-class="transform opacity-100 scale-100"
            leave-active-class="transition ease-in duration-75" 
            leave-from-class="transform opacity-100 scale-100" 
            leave-to-class="transform opacity-0 scale-95"
        >
            <div v-if="showDropdown" class="absolute left-0 right-0 top-full mt-1 bg-[#1E1E2D] border border-gray-600 rounded-xl shadow-2xl z-50 overflow-hidden max-h-60 overflow-y-auto custom-scrollbar">
                
                <div v-if="isLoading && productList.length === 0" class="p-4 space-y-3">
                    <div class="h-4 bg-gray-700/50 rounded w-3/4 animate-pulse"></div>
                    <div class="h-4 bg-gray-700/50 rounded w-1/2 animate-pulse"></div>
                </div>

                <div v-else-if="fetchError" class="p-4 text-center text-red-400 text-xs flex items-center justify-center gap-2">
                    <span>⚠️</span> Arama sırasında hata oluştu.
                </div>

                <div v-else-if="productList.length === 0" class="p-6 text-center text-gray-500 text-xs flex flex-col items-center">
                    <span class="text-2xl mb-1">🤷‍♂️</span>
                    Sonuç bulunamadı.
                </div>
                
                <ul v-else class="divide-y divide-gray-700/30">
                    <li 
                        v-for="(prod, index) in productList" 
                        :key="prod.id"
                        @mousedown="selectProduct(prod)" 
                        @mouseenter="focusedIndex = index"
                        class="px-4 py-3 cursor-pointer flex justify-between items-center group transition-all duration-150"
                        :class="[
                            index === focusedIndex ? 'bg-indigo-500/20 dropdown-item-active pl-5' : 'hover:bg-indigo-500/10'
                        ]"
                    >
                        <div>
                            <div class="text-sm font-bold transition-colors" :class="index === focusedIndex ? 'text-indigo-300' : 'text-gray-200'">
                                {{ prod.name }}
                            </div>
                            <div class="text-[10px] text-gray-500 mt-0.5 flex items-center gap-2">
                                <span v-if="prod.sku" class="bg-gray-700/50 text-gray-400 px-1.5 py-0.5 rounded font-mono border border-gray-600">
                                    {{ prod.sku }}
                                </span>
                                <span>Stok: {{ prod.stock }}</span>
                            </div>
                        </div>
                        
                        <div v-if="index === focusedIndex" class="text-indigo-400 text-xs font-bold">
                            Seç ↵
                        </div>
                    </li>
                </ul>
            </div>
        </transition>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 5px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #1E1E2D; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #3f3f4e; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #555566; }
</style>