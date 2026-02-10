<script setup lang="ts">
import { ref, watch } from 'vue';

// --- TYPES ---
interface WidgetItem {
    key: string;
    label: string;
}

interface Props {
    isOpen: boolean;
    initialSettings?: string[];
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    initialSettings: () => []
});

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save', settings: string[]): void;
}>();

// Tüm Mevcut Widget'lar
const allAvailableWidgets: WidgetItem[] = [
    { key: 'quickActions', label: 'Hızlı İşlemler' },
    { key: 'totalRevenue', label: 'Toplam Ciro' },
    { key: 'totalOrders', label: 'Toplam Sipariş' },
    { key: 'totalProducts', label: 'Toplam Ürün' },
    { key: 'totalCustomers', label: 'Müşteri Sayısı' },
    { key: 'totalStockCount', label: 'Toplam Stok' },
    { key: 'criticalStockCount', label: 'Kritik Stok' },
    { key: 'outOfStockCount', label: 'Tükenenler' },
    { key: 'inventoryValue', label: 'Envanter Değeri' },
    { key: 'revenueChart', label: 'Gelir Grafiği' },
    { key: 'topSelling', label: 'En Çok Satanlar' },
    { key: 'recentOrders', label: 'Son Siparişler' }
];

const localSettings = ref<string[]>([]);
const originalSettings = ref<string[]>([]);

watch(
    () => props.isOpen, 
    (newVal) => {
        if (newVal) {
            if (props.initialSettings && props.initialSettings.length > 0) {
                localSettings.value = [...props.initialSettings];
            } else {
                localSettings.value = allAvailableWidgets.map(w => w.key);
            }
            originalSettings.value = [...localSettings.value];
        }
    }
);

const isActive = (key: string): boolean => localSettings.value.includes(key);

const toggleWidget = (key: string) => {
    if (isActive(key)) {
        localSettings.value = localSettings.value.filter(k => k !== key);
    } else {
        localSettings.value.push(key);
    }
};

const moveItem = (index: number, direction: number) => {
    if (direction === -1 && index === 0) return;
    if (direction === 1 && index === localSettings.value.length - 1) return;

    const newIndex = index + direction;
    const temp = localSettings.value[index];
    localSettings.value[index] = localSettings.value[newIndex];
    localSettings.value[newIndex] = temp;
};

const getLabel = (key: string): string => {
    return allAvailableWidgets.find(w => w.key === key)?.label || key;
};

const handleSave = () => {
    const hasChanged = JSON.stringify(localSettings.value) !== JSON.stringify(originalSettings.value);
    if (hasChanged) {
        emit('save', localSettings.value);
    }
    emit('close');
};
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-[100] overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
        <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
            
            <div class="fixed inset-0 transition-opacity bg-gray-900/80 backdrop-blur-sm" @click="$emit('close')"></div>

            <span class="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

            <div class="inline-block w-full text-left align-bottom transition-all transform bg-[#1E1E2D]/90 backdrop-blur-xl border border-[#2B2B40]/50 rounded-xl shadow-2xl shadow-black/50 sm:my-8 sm:align-middle sm:max-w-3xl relative overflow-hidden">
                
                <div class="flex items-center justify-between px-6 py-4 border-b border-[#2B2B40]/50 bg-[#151521]/80">
                    <div>
                        <h3 class="text-lg font-bold text-white">Dashboard Düzeni</h3>
                        <p class="text-xs text-gray-400 mt-0.5">Görüntülenecek kartları seçin.</p>
                    </div>
                    <button @click="$emit('close')" class="text-gray-400 hover:text-white transition-colors bg-white/5 hover:bg-white/10 p-1.5 rounded-lg border border-white/10">
                        <svg xmlns="http://www.w3.org/2000/svg" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                <div class="px-6 py-6 max-h-[70vh] overflow-y-auto custom-scrollbar">
                    
                    <div class="mb-8">
                        <h4 class="flex items-center gap-2 mb-3 text-xs font-bold tracking-wider text-green-400 uppercase">
                            <span class="w-2 h-2 bg-green-500 rounded-full"></span>
                            Aktif Kartlar ({{ localSettings.length }})
                        </h4>
                        
                        <div class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
                            <div v-for="(key, index) in localSettings" :key="key" 
                                class="flex items-center justify-between p-3 transition-all border rounded-lg bg-[#151521]/60 border-[#374151]/50 hover:border-indigo-500/50 group">
                                
                                <div class="flex items-center gap-3 overflow-hidden">
                                    <input type="checkbox" checked @change="toggleWidget(key)" 
                                           class="w-4 h-4 text-indigo-600 bg-gray-700 border-gray-600 rounded cursor-pointer form-checkbox focus:ring-indigo-500 focus:ring-offset-gray-900">
                                    <span class="text-sm font-medium text-gray-200 truncate" :title="getLabel(key)">{{ getLabel(key) }}</span>
                                </div>

                                <div class="flex gap-1 opacity-50 group-hover:opacity-100">
                                    <button @click="moveItem(index, -1)" :disabled="index === 0" 
                                            class="p-1 hover:bg-white/10 rounded text-gray-400 hover:text-white disabled:opacity-0 transition-all">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
                                        </svg>
                                    </button>
                                    <button @click="moveItem(index, 1)" :disabled="index === localSettings.length - 1" 
                                            class="p-1 hover:bg-white/10 rounded text-gray-400 hover:text-white disabled:opacity-0 transition-all">
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                                        </svg>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div v-if="allAvailableWidgets.length > localSettings.length">
                        <h4 class="flex items-center gap-2 mb-3 text-xs font-bold tracking-wider text-gray-500 uppercase">
                            <span class="w-2 h-2 bg-gray-600 rounded-full"></span>
                            Gizlenen Kartlar
                        </h4>
                        <div class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
                            <div v-for="widget in allAvailableWidgets.filter(w => !isActive(w.key))" :key="widget.key" 
                                class="flex items-center gap-3 p-3 transition-all border border-dashed rounded-lg opacity-60 bg-[#151521]/30 border-[#374151]/50 hover:opacity-100 hover:bg-[#151521]/60 hover:border-gray-500 cursor-pointer"
                                @click="toggleWidget(widget.key)">
                                <div class="flex items-center justify-center w-4 h-4 border border-gray-600 rounded bg-gray-800"></div>
                                <span class="text-sm text-gray-400">{{ widget.label }}</span>
                            </div>
                        </div>
                    </div>

                </div>

                <div class="px-6 py-4 bg-[#151521]/80 border-t border-[#2B2B40]/50 flex justify-end gap-3">
                    <button @click="$emit('close')" type="button" class="px-4 py-2 text-sm font-medium text-gray-300 transition-colors bg-white/5 hover:bg-white/10 border border-white/10 rounded-lg">
                        Vazgeç
                    </button>
                    <button @click="handleSave" type="button" class="px-6 py-2 text-sm font-medium text-white transition-colors bg-indigo-600 rounded-lg hover:bg-indigo-700 shadow-lg shadow-indigo-500/20">
                        Kaydet
                    </button>
                </div>

            </div>
        </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
    width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
    background: rgba(21, 21, 33, 0.5); 
}
.custom-scrollbar::-webkit-scrollbar-thumb {
    background: #374151;
    border-radius: 3px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: #4b5563;
}
</style>