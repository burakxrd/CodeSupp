<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';

// --- TYPES ---
interface ColorTheme {
    text: string;
    border: string;
    bg: string;
}

interface Props {
    modelValue: number;
    label?: string;
    color?: string; 
    allowPercentage?: boolean;
    baseAmount?: number;
    defaultRate?: number;
    icon?: string;
}

// --- PROPS & EMITS ---
const props = withDefaults(defineProps<Props>(), {
    modelValue: 0,
    color: 'blue',
    allowPercentage: false,
    baseAmount: 0,
    defaultRate: 0,
    icon: 'TL'
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: number): void;
}>();

// --- STATE ---
const mode = ref<'TL' | '%'>('TL');
const rate = ref<number>(0);

// --- COLORS ---
// Record tipi kullanarak index signature hatasını önlüyoruz
const colors: Record<string, ColorTheme> = {
    orange: { text: 'text-orange-400', border: 'focus:border-orange-500', bg: 'bg-orange-600' },
    purple: { text: 'text-purple-400', border: 'focus:border-purple-500', bg: 'bg-purple-600' },
    cyan:   { text: 'text-cyan-400',   border: 'focus:border-cyan-500',   bg: 'bg-cyan-600' },
    red:    { text: 'text-red-400',    border: 'focus:border-red-500',    bg: 'bg-red-600' },
    blue:   { text: 'text-blue-400',   border: 'focus:border-blue-500',   bg: 'bg-blue-600' },
};

// Fallback olarak blue kullanıyoruz
const activeColor = colors[props.color] || colors.blue;

// --- ACTIONS ---
const handleFocus = (val: number) => (val === 0 ? '' : val);
const handleBlur = (val: number | string) => (!val ? 0 : Number(val));

const calculateFromRate = () => {
    if (mode.value === '%') {
        const calculated = (props.baseAmount * rate.value) / 100;
        emit('update:modelValue', calculated);
    }
};

const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
};

// Baz Tutar (productsTotal) değişirse ve mod % ise yeniden hesapla
watch(() => props.baseAmount, () => {
    if (mode.value === '%') calculateFromRate();
});

// Varsayılan oran varsa uygula
onMounted(() => {
    if (props.defaultRate > 0 && props.allowPercentage) {
        rate.value = props.defaultRate;
        mode.value = '%';
        calculateFromRate();
    }
});
</script>

<template>
    <div class="animate-fade-in">
        <div class="flex justify-between items-center mb-2">
            <label :class="['block text-xs uppercase font-bold', activeColor.text]">{{ label }}</label>
            
            <div v-if="allowPercentage" class="flex bg-gray-800 rounded p-0.5 border border-gray-600">
                <button type="button" @click="mode = 'TL'" :class="mode === 'TL' ? `${activeColor.bg} text-white` : 'text-gray-400'" class="px-2 py-0.5 text-[10px] rounded font-bold transition-all">TL</button>
                <button type="button" @click="mode = '%'; calculateFromRate()" :class="mode === '%' ? `${activeColor.bg} text-white` : 'text-gray-400'" class="px-2 py-0.5 text-[10px] rounded font-bold transition-all">%</button>
            </div>
        </div>

        <div class="relative">
            <div v-if="mode === 'TL'" class="relative group">
                <input 
                    :value="modelValue"
                    @input="emit('update:modelValue', parseFloat(($event.target as HTMLInputElement).value) || 0)"
                    @focus="($event.target as HTMLInputElement).value = String(handleFocus(modelValue))"
                    @blur="($event.target as HTMLInputElement).value = String(handleBlur(modelValue))"
                    type="number" min="0" step="0.01" 
                    :class="['w-full bg-gray-800 border border-gray-600 rounded-lg px-3 py-3 text-white text-sm text-right outline-none transition-colors group-hover:border-gray-500', activeColor.border]"
                >
                <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-xs font-bold pointer-events-none">TL</span>
            </div>

            <div v-else class="flex items-center gap-2">
                <div class="relative flex-1 group">
                    <input 
                        v-model.number="rate" 
                        @input="calculateFromRate"
                        @focus="rate = Number(handleFocus(rate))"
                        @blur="rate = handleBlur(rate)"
                        type="number" min="0" max="100" step="0.1" 
                        :class="['w-full bg-gray-800 border border-gray-600 rounded-lg px-3 py-3 text-white text-sm text-right outline-none transition-colors group-hover:border-gray-500', activeColor.border]"
                        placeholder="%"
                    >
                    <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 text-xs font-bold pointer-events-none">%</span>
                </div>
                <div class="text-xs text-gray-400 font-mono bg-gray-800 px-2 py-3 rounded border border-gray-700 min-w-[80px] text-right">
                    = {{ formatCurrency(modelValue) }}
                </div>
            </div>
        </div>
        
        <slot name="footer"></slot>
    </div>
</template>

<style scoped>
input::-webkit-outer-spin-button, input::-webkit-inner-spin-button { -webkit-appearance: none; margin: 0; }
</style>