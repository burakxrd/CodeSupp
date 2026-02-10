<script setup lang="ts">
import { watch } from 'vue';
import type { Customer } from '../../types'; // Global tipler

// --- LOGIC ---
import { useCustomerForm } from '../../composables/forms/useCustomerForm';

// --- UI COMPONENTS ---
import BaseInput from '../ui/BaseInput.vue';
import BaseTextarea from '../ui/BaseTextarea.vue';
import BaseButton from '../ui/BaseButton.vue';

// --- PROPS ---
interface Props {
    isOpen: boolean;
    customerToEdit?: Customer | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    customerToEdit: null
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'save-success'): void;
}>();

// Composable'dan gelenleri kullanıyoruz
// (Composable'ın dönüş tipleri zaten orada tanımlı olmalı)
const { 
    formData, 
    errors, 
    isLoading, 
    setForm, 
    handleSubmit, 
    validate 
} = useCustomerForm();

// Modal açıldığında form verisini yükle
watch(
    () => props.isOpen, 
    (isOpen) => {
        if (isOpen) {
            setForm(props.customerToEdit);
        }
    }
);

const onSubmit = () => {
    handleSubmit(() => {
        emit('save-success');
        emit('close');
    });
};
</script>

<template>
    <Teleport to="body">
        <div v-if="isOpen" class="fixed inset-0 z-[999] flex items-center justify-center p-4">
            
            <div 
                class="fixed inset-0 bg-black/60 backdrop-blur-sm transition-opacity" 
                @click="$emit('close')"
            ></div>
            
            <div class="bg-[#1E1E2D] w-full max-w-lg rounded-xl shadow-2xl border border-gray-700 p-6 relative z-10 animate-zoom-in">
                
                <div class="flex justify-between items-center mb-6 border-b border-gray-700 pb-4">
                    <h3 class="text-xl font-bold text-white flex items-center gap-2">
                        <span v-if="customerToEdit">✏️</span>
                        <span v-else>👤</span>
                        {{ customerToEdit ? 'Müşteriyi Düzenle' : 'Yeni Müşteri Ekle' }}
                    </h3>
                    <button 
                        @click="$emit('close')" 
                        class="text-gray-400 hover:text-white transition-colors p-1 rounded-md hover:bg-white/10"
                        title="Kapat"
                    >
                        ✕
                    </button>
                </div>

                <form @submit.prevent="onSubmit" class="space-y-5">
                    
                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <BaseInput 
                            label="Ad Soyad" 
                            v-model="formData.name" 
                            :error="errors.name" 
                            placeholder="Örn: Ahmet Yılmaz"
                            required
                            @blur="validate" 
                        />
                        
                        <BaseInput 
                            label="Instagram" 
                            v-model="formData.instagramHandle" 
                            :error="errors.instagramHandle" 
                            placeholder="@kullanici" 
                        />
                    </div>

                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <BaseInput 
                            label="Telefon" 
                            v-model="formData.phone" 
                            :error="errors.phone" 
                            type="tel" 
                            placeholder="0555 123 45 67" 
                        />
                        
                        <BaseInput 
                            label="E-posta" 
                            v-model="formData.email" 
                            :error="errors.email" 
                            type="email" 
                            placeholder="ornek@mail.com"
                            @blur="validate"
                        />
                    </div>

                    <div>
                        <BaseTextarea 
                            label="Adres" 
                            v-model="formData.address" 
                            :error="errors.address" 
                            rows="3" 
                            placeholder="Tam adres bilgisi..." 
                        />
                    </div>

                    <div class="flex justify-end gap-3 pt-4 border-t border-gray-700">
                        <BaseButton 
                            type="button" 
                            variant="secondary" 
                            @click="$emit('close')"
                        >
                            İptal
                        </BaseButton>

                        <BaseButton 
                            type="submit" 
                            variant="primary" 
                            :loading="isLoading"
                        >
                            {{ customerToEdit ? 'Güncelle' : 'Kaydet' }}
                        </BaseButton>
                    </div>
                </form>
            </div>
        </div>
    </Teleport>
</template>

<style scoped>
/* Yumuşak Giriş Animasyonu */
.animate-zoom-in {
    animation: zoomIn 0.2s ease-out forwards;
}

@keyframes zoomIn {
    from { 
        opacity: 0; 
        transform: scale(0.95) translateY(10px); 
    }
    to { 
        opacity: 1; 
        transform: scale(1) translateY(0); 
    }
}
</style>