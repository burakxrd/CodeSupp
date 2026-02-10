<script setup lang="ts">
import { ref, watch } from 'vue';

// --- TYPES & PROPS ---
interface Props {
    modelValue?: number; // v-model:stock için
    reason?: string;    // v-model:reason için
    editMode: boolean;
    originalStock?: number;
    productType?: number; // 0: Fiziksel, 1: Dijital, 2: Hizmet
}

const props = withDefaults(defineProps<Props>(), {
    modelValue: 0,
    reason: '',
    originalStock: 0,
    productType: 0
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'update:modelValue', value: number): void;
    (e: 'update:reason', value: string): void;
}>();

// --- STATE ---
// Kilit durumu sadece bu bileşeni ilgilendirir, ana modalın bilmesine gerek yok
const isUnlocked = ref<boolean>(false);

// Edit modu açıldığında/kapandığında kilidi sıfırla
watch(() => props.editMode, () => {
    isUnlocked.value = false;
});

// --- HANDLERS ---
const handleUnlock = () => {
    isUnlocked.value = true;
};

const handleCancel = () => {
    isUnlocked.value = false;
    // Stoğu eski haline getir ve sebebi temizle
    emit('update:modelValue', props.originalStock);
    emit('update:reason', '');
};

const handleStockInput = (event: Event) => {
    const target = event.target as HTMLInputElement;
    const val = target.value === '' ? 0 : parseFloat(target.value);
    emit('update:modelValue', val);
};

const handleReasonInput = (event: Event) => {
    const target = event.target as HTMLInputElement;
    emit('update:reason', target.value);
};
</script>

<template>
    <div class="bg-[#151521] p-3 rounded-lg border border-gray-700/50">
        <div class="flex justify-between items-center mb-1">
            <label class="block text-sm font-medium text-gray-400">
                Stok Adedi
                <span v-if="productType === 2" class="text-xs text-gray-500 ml-1">(Hizmet için genelde takibi yapılmaz)</span>
            </label>
            
            <button 
                v-if="editMode && !isUnlocked" 
                type="button" 
                @click="handleUnlock" 
                class="text-xs text-indigo-400 hover:text-indigo-300 flex items-center gap-1 transition-colors"
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clip-rule="evenodd" />
                </svg>
                Düzeltme Yap
            </button>
            
            <button 
                v-if="editMode && isUnlocked" 
                type="button" 
                @click="handleCancel" 
                class="text-xs text-red-400 hover:text-red-300 transition-colors"
            >
                İptal Et
            </button>
        </div>

        <input 
            :value="modelValue"
            @input="handleStockInput"
            type="number" 
            :disabled="editMode && !isUnlocked"
            class="w-full bg-[#1E1E2D] border border-gray-600 rounded-md py-2 px-3 text-white focus:outline-none focus:ring-2 focus:ring-indigo-500 disabled:opacity-60 disabled:cursor-not-allowed transition-all"
        >

        <div v-if="editMode && isUnlocked" class="mt-3 animate-fade-in">
            <div class="flex items-center gap-2 mb-1">
                <label class="text-xs font-medium text-yellow-500">Düzeltme Sebebi (Zorunlu)</label>
                
                <div class="relative group cursor-help">
                    <div class="w-4 h-4 rounded-full border border-gray-500 text-gray-500 flex items-center justify-center text-[10px] font-serif italic hover:border-gray-300 hover:text-gray-300 transition-colors">i</div>
                    <div class="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-56 p-3 bg-[#151521] border border-gray-600 text-gray-300 text-xs rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 z-10 text-center leading-relaxed pointer-events-none">
                        Bu işlem güvenliğiniz için <strong>"Stok Hareketleri"</strong> geçmişine kaydedilecek ve raporlarda görünecektir.
                        <div class="absolute top-full left-1/2 -translate-x-1/2 -mt-1 border-4 border-transparent border-t-gray-600"></div>
                    </div>
                </div>
            </div>
            
            <input 
                :value="reason" 
                @input="handleReasonInput"
                type="text" 
                placeholder="Örn: Sayım eksiği, Kırık ürün..." 
                class="w-full bg-[#2B2B40] border border-yellow-500/50 rounded-md py-2 px-3 text-white text-sm focus:outline-none focus:ring-1 focus:ring-yellow-500 transition-all"
            >
        </div>
    </div>
</template>

<style scoped>
.animate-fade-in { animation: fadeIn 0.3s ease-in-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(-5px); } to { opacity: 1; transform: translateY(0); } }
</style>