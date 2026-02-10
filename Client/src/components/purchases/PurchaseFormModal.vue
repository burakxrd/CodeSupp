<script setup lang="ts">
import { watch } from 'vue';
import { usePurchaseForm } from '../../composables/forms/usePurchaseForm'; 

// COMPONENT IMPORTS
import BaseInput from '../../components/ui/BaseInput.vue';
import BaseButton from '../../components/ui/BaseButton.vue';
import PurchaseSearchInput from './PurchaseSearchInput.vue'; 
import type { Purchase } from '../../types';

// --- TYPES ---
interface Props {
    isOpen: boolean;
    purchaseToEdit?: Purchase | null;
}

const props = withDefaults(defineProps<Props>(), {
    purchaseToEdit: null
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save-success'): void;
}>();

// LOGIC (Composable)
const { 
    form, 
    isEditing, 
    isLoading,
    searchQuery, 
    totalCost,
    initForm, 
    submitForm 
} = usePurchaseForm(props, emit);

// --- WATCH ---
watch(() => props.isOpen, (isOpen) => {
    if (isOpen) initForm();
}, { immediate: true });

// Helper
const formatCurrency = (val: number): string => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
</script>

<template>
    <Teleport to="body">
        <div v-if="isOpen" class="fixed inset-0 z-[999] overflow-y-auto" role="dialog" aria-modal="true">
            <div class="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
                
                <transition 
                    enter-active-class="transition-opacity ease-out duration-300"
                    enter-from-class="opacity-0"
                    enter-to-class="opacity-100"
                    leave-active-class="transition-opacity ease-in duration-200"
                    leave-from-class="opacity-100"
                    leave-to-class="opacity-0"
                >
                    <div 
                        class="fixed inset-0 bg-gray-900/80 backdrop-blur-sm" 
                        @click="emit('close')"
                    ></div>
                </transition>

                <span class="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

                <transition 
                    enter-active-class="transition-all transform ease-out duration-300"
                    enter-from-class="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
                    enter-to-class="opacity-100 translate-y-0 sm:scale-100"
                    leave-active-class="transition-all transform ease-in duration-200"
                    leave-from-class="opacity-100 translate-y-0 sm:scale-100"
                    leave-to-class="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
                >
                    <div class="inline-block align-bottom bg-[#1E1E2D] rounded-xl text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-xl w-full border border-gray-700/50 relative z-[1000]">
                        
                        <div class="px-6 py-4 border-b border-gray-700/50 bg-[#151521] flex justify-between items-center">
                            <h3 class="text-lg font-bold text-white tracking-wide flex items-center gap-2">
                                <span>📦</span> {{ isEditing ? 'Kaydı Düzenle' : 'Yeni Mal Alımı' }}
                            </h3>
                            <button 
                                @click="emit('close')" 
                                class="text-gray-400 hover:text-white transition-colors p-1 rounded-lg hover:bg-white/5"
                            >
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>

                        <div class="px-6 py-6 space-y-6">
                            
                            <PurchaseSearchInput 
                                v-model="form.productId"
                                :initial-name="searchQuery"
                                @update:name="(name: string) => searchQuery = name"
                            />

                            <div class="border-t border-gray-700/50"></div>

                             <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <BaseInput 
                                    label="Satın Alma Tarihi" 
                                    v-model="form.purchaseDate" 
                                    type="date" 
                                    required 
                                />
                                <BaseInput 
                                    label="Açıklama / Fiş No" 
                                    v-model="form.description" 
                                    type="text" 
                                    placeholder="Opsiyonel (Boşsa '-' gider)"
                                />
                            </div>

                            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                <BaseInput 
                                    label="Adet (Units)" 
                                    v-model.number="form.quantityInUnits" 
                                    type="number" 
                                    min="1" 
                                    required 
                                    placeholder="0"
                                />
                                <BaseInput 
                                    label="Birim Alış Fiyatı (TL)" 
                                    v-model.number="form.productPricePerUnit" 
                                    type="number" 
                                    step="0.01" 
                                    placeholder="0.00"
                                />
                            </div>

                            <div class="bg-[#151521] p-4 rounded-xl border border-gray-700/50">
                                <h4 class="text-xs font-bold text-gray-400 uppercase mb-3 tracking-wider flex items-center gap-2">
                                    <span>🚚</span> Lojistik Maliyetleri
                                </h4>
                                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                                    <BaseInput 
                                        label="Toplam Ağırlık (Kg)" 
                                        v-model.number="form.totalKg" 
                                        type="number" 
                                        step="0.01" 
                                        placeholder="0.00"
                                    />
                                    <BaseInput 
                                        label="Kargo Birim (TL/Kg)" 
                                        v-model.number="form.shippingCostPerKg" 
                                        type="number" 
                                        step="0.01" 
                                        placeholder="0.00"
                                    />
                                </div>
                            </div>

                            <div class="flex items-center justify-between p-4 bg-emerald-900/10 border border-emerald-500/20 rounded-xl">
                                <span class="text-emerald-400/80 text-sm font-medium uppercase tracking-wide">Tahmini Toplam Maliyet</span>
                                <span class="text-2xl font-bold text-emerald-400 font-mono tracking-tight">
                                    {{ formatCurrency(totalCost) }}
                                </span>
                            </div>
                        </div>

                        <div class="px-6 py-4 bg-[#151521] border-t border-gray-700/50 flex justify-end gap-3">
                            <BaseButton variant="secondary" @click="emit('close')">İptal</BaseButton>
                            <BaseButton variant="success" :loading="isLoading" @click="submitForm">
                                {{ isEditing ? 'Kaydı Güncelle' : 'Satın Alımı Onayla' }}
                            </BaseButton>
                        </div>

                    </div>
                </transition>
            </div>
        </div>
    </Teleport>
</template>