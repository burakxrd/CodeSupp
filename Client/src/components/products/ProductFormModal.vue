<script setup lang="ts">
import { ref, watch } from 'vue';
import { useProductForm } from '../../composables/forms/useProductForm'; 
import type { Product } from '../../types';

// UI Components
import BaseInput from '../ui/BaseInput.vue';
import BaseSelect from '../ui/BaseSelect.vue';
import BaseTextarea from '../ui/BaseTextarea.vue';
import BaseButton from '../ui/BaseButton.vue';
import ProductImageUpload from './ProductImageUpload.vue';
import ProductStockInput from './ProductStockInput.vue';

// --- TYPES ---
interface Props {
    isOpen: boolean;
    editMode: boolean;
    productData?: Product | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    editMode: false,
    productData: null
});

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save-success'): void;
}>();

// 1. Logic Import
// Not: useProductForm henüz TS değilse 'form' nesnesi 'any' olabilir.
// Bir sonraki adımda useProductForm'u dönüştürmeliyiz.
const { 
    form, 
    errors,
    categories, 
    loadingCategories, 
    submitting, 
    adjustmentReason, 
    originalStock,
    discountRate,
    isClothingCategory,
    productTypes,
    shippingTypes,
    initForm, 
    submitForm,
    calculateRate,
    calculatePriceFromRate
} = useProductForm();

// 2. UI State
const selectedImage = ref<File | null>(null);

// 3. Modal Kontrolü
watch(
    () => props.isOpen, 
    (isOpen) => {
        if (isOpen) {
            // initForm'un parametre tipini composable'da belirleyeceğiz
            initForm(props.productData);
            selectedImage.value = null;
        }
    },
    { immediate: true }
);

// 4. Kaydet
const handleSave = () => {
    // submitForm'a dosya ve callback gönderiyoruz
    submitForm(selectedImage.value, () => {
        emit('save-success');
        emit('close');
    });
};
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-50 overflow-y-auto" role="dialog" aria-modal="true">
        <div class="fixed inset-0 bg-gray-900/80 backdrop-blur-sm transition-opacity" @click="$emit('close')"></div>

        <div class="flex min-h-full items-center justify-center p-4 text-center sm:p-0">
            
            <div class="relative transform overflow-hidden rounded-2xl bg-[#1E1E2D] text-left shadow-2xl transition-all sm:my-8 sm:w-full sm:max-w-2xl border border-gray-700/50">
                
                <div class="flex items-center justify-between px-6 py-4 border-b border-gray-700/50 bg-[#151521]">
                    <h3 class="text-lg font-bold text-white tracking-wide">
                        {{ editMode ? '📦 Ürünü Düzenle' : '✨ Yeni Ürün Ekle' }}
                    </h3>
                    <button 
                        @click="$emit('close')" 
                        class="rounded-lg p-1 text-gray-400 hover:bg-white/10 hover:text-white transition-colors"
                    >
                        <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                <div class="px-6 py-6 space-y-6 max-h-[75vh] overflow-y-auto custom-scrollbar">
                    
                    <div class="bg-[#151521]/50 p-4 rounded-xl border border-gray-700/30">
                        <ProductImageUpload 
                            :initial-image="form.imagePath"
                            @change="(file: File | null) => selectedImage = file"
                        />
                    </div>

                    <div class="space-y-4">
                        <BaseInput 
                            label="Ürün Adı" 
                            v-model="form.name" 
                            :error="errors.name" 
                            placeholder="Örn: Kablosuz Kulaklık" 
                            required 
                        />

                        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                            <BaseSelect 
                                label="Kategori" 
                                v-model="form.categoryId"
                                :options="categories"
                                :error="errors.categoryId"
                                :loading="loadingCategories"
                                valueKey="id"
                                labelKey="name"
                                placeholder="Kategori Seçiniz"
                                required
                            />

                            <div>
                                <BaseSelect 
                                    label="Ürün Tipi" 
                                    v-model="form.type"
                                    :options="productTypes"
                                    valueKey="id"
                                    labelKey="name"
                                    :disabled="isClothingCategory"
                                />
                                <p v-if="isClothingCategory" class="text-[11px] text-indigo-400 mt-1.5 flex items-center gap-1">
                                    <span>ℹ️</span> Giyim kategorisi fizikseldir.
                                </p>
                            </div>
                        </div>
                    </div>

                    <transition name="fade">
                        <div v-if="isClothingCategory" class="bg-indigo-900/10 p-4 rounded-xl border border-indigo-500/20">
                            <div class="flex items-center gap-2 mb-3">
                                <span class="text-lg">👕</span>
                                <h4 class="text-xs font-bold text-indigo-300 uppercase tracking-wider">Giyim Özellikleri</h4>
                            </div>
                            <div class="grid grid-cols-2 gap-4">
                                <BaseInput label="Beden" v-model="form.size" placeholder="S, M, L..." />
                                <BaseInput label="Renk" v-model="form.color" placeholder="Kırmızı, Mavi..." />
                            </div>
                        </div>
                    </transition>

                    <div class="border-t border-gray-700/50"></div>

                    <div>
                        <h4 class="text-xs font-bold text-gray-400 mb-4 uppercase tracking-wider flex items-center gap-2">
                            <span>💰</span> Fiyatlandırma
                        </h4>
                        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                            <BaseInput label="Stok Kodu" v-model="form.code" :error="errors.code" placeholder="Otomatik" />
                            
                            <BaseInput 
                                label="Alış Fiyatı" 
                                v-model.number="form.costPrice" 
                                type="number" 
                                step="0.01" 
                                placeholder="0.00" 
                            />
                            <BaseInput 
                                label="Satış Fiyatı" 
                                v-model.number="form.price" 
                                :error="errors.price" 
                                type="number" 
                                step="0.01" 
                                required 
                                placeholder="0.00" 
                            />
                        </div>

                        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mt-4 bg-[#151521] p-4 rounded-xl border border-gray-700/50">
                            <BaseInput 
                                label="İndirim Oranı (%)" 
                                v-model.number="discountRate" 
                                @input="calculatePriceFromRate" 
                                type="number" 
                                placeholder="%0" 
                            />
                            <BaseInput 
                                label="İndirimli Fiyat" 
                                v-model.number="form.discountedPrice" 
                                @input="calculateRate" 
                                type="number" 
                                step="0.01" 
                                placeholder="Opsiyonel" 
                            />
                        </div>
                    </div>

                    <div class="border-t border-gray-700/50"></div>

                    <div>
                        <h4 class="text-xs font-bold text-gray-400 mb-4 uppercase tracking-wider flex items-center gap-2">
                            <span>📦</span> Lojistik
                        </h4>
                        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
                            <BaseSelect 
                                label="Kargo Tipi" 
                                v-model="form.shippingType"
                                :options="shippingTypes"
                                valueKey="id"
                                labelKey="name"
                            />
                            <BaseInput 
                                label="Kargo Ücreti" 
                                v-model.number="form.shippingPrice" 
                                type="number" 
                                step="0.01" 
                                :disabled="form.shippingType === 0" 
                            />
                        </div>

                        <ProductStockInput 
                            v-model="form.stock"
                            v-model:reason="adjustmentReason"
                            :edit-mode="editMode"
                            :original-stock="originalStock"
                            :product-type="form.type"
                            :error="errors.stock"
                        />
                    </div>

                    <BaseTextarea 
                        label="Ürün Açıklaması" 
                        v-model="form.description" 
                        :error="errors.description" 
                        rows="3" 
                        placeholder="Ürün hakkında detaylı bilgi..." 
                    />

                </div>

                <div class="px-6 py-4 bg-[#151521] border-t border-gray-700/50 flex justify-end gap-3">
                    <BaseButton variant="secondary" @click="$emit('close')">İptal</BaseButton>
                    <BaseButton variant="primary" :loading="submitting" @click="handleSave">
                        {{ editMode ? 'Değişiklikleri Kaydet' : 'Ürünü Oluştur' }}
                    </BaseButton>
                </div>

            </div>
        </div>
    </div>
</template>

<style scoped>
/* Scrollbar Güzelleştirme */
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #1E1E2D; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #3f3f4e; border-radius: 10px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #555566; }

/* Transition Animasyonları */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>