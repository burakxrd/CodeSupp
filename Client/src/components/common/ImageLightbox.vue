<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';

// --- PROPS ---
interface Props {
    show: boolean;
    imageSrc?: string | null; // null da gelebilir
}

const props = withDefaults(defineProps<Props>(), {
    show: false,
    imageSrc: ''
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'close'): void;
}>();

// --- KEYBOARD SUPPORT (ESC) ---
const handleKeydown = (e: KeyboardEvent) => {
    if (e.key === 'Escape' && props.show) {
        emit('close');
    }
};

onMounted(() => document.addEventListener('keydown', handleKeydown));
onUnmounted(() => document.removeEventListener('keydown', handleKeydown));
</script>

<template>
    <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
    >
        <div 
            v-if="show" 
            class="fixed inset-0 z-[9999] bg-black/95 backdrop-blur-sm flex items-center justify-center p-4" 
            @click="$emit('close')"
        >
            <div class="relative max-w-7xl max-h-screen" @click.stop>
                
                <img 
                    v-if="imageSrc"
                    :src="imageSrc" 
                    class="max-h-[85vh] max-w-full rounded-lg shadow-2xl border border-gray-800 object-contain select-none" 
                    alt="Preview"
                >
                
                <button 
                    @click="$emit('close')" 
                    class="absolute -top-10 -right-2 text-gray-400 hover:text-white text-3xl font-bold transition-colors w-8 h-8 flex items-center justify-center rounded-full hover:bg-white/10"
                    title="Kapat (Esc)"
                >
                    &times;
                </button>
                
                <p class="text-center text-gray-500 text-xs mt-3 font-mono opacity-60">
                    Kapatmak için boşluğa tıklayın veya ESC tuşuna basın
                </p>
            </div>
        </div>
    </Transition>
</template>