<script setup lang="ts">
// --- TYPES ---
interface FilterOption {
    key: string | null;
    label: string;
    icon: string;
    textClass: string;
    bgGlow: string;
    border: string;
}

// --- PROPS ---
interface Props {
    modelValue?: string | null;
}

const props = withDefaults(defineProps<Props>(), {
    modelValue: null
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'update:modelValue', value: string | null): void;
}>();

// --- STATE ---
const filters: FilterOption[] = [
    { key: null, label: 'Tümü', icon: '📋', textClass: 'text-indigo-400', bgGlow: 'bg-indigo-500', border: 'border-indigo-500/30' },
    { key: 'vip', label: 'VIP', icon: '👑', textClass: 'text-purple-400', bgGlow: 'bg-purple-500', border: 'border-purple-500/30' },
    { key: 'sadik', label: 'Sadık', icon: '🤝', textClass: 'text-blue-400', bgGlow: 'bg-blue-500', border: 'border-blue-500/30' },
    { key: 'yeni', label: 'Yeni', icon: '🌱', textClass: 'text-emerald-400', bgGlow: 'bg-emerald-500', border: 'border-emerald-500/30' },
    { key: 'riskli', label: 'Riskli', icon: '⚠️', textClass: 'text-orange-400', bgGlow: 'bg-orange-500', border: 'border-orange-500/30' },
    { key: 'plus75', label: '+75 Gün', icon: '🚫', textClass: 'text-red-400', bgGlow: 'bg-red-500', border: 'border-red-500/30' },
];

// --- ACTIONS ---
const setFilter = (key: string | null) => {
    emit('update:modelValue', key);
};
</script>

<template>
    <div class="flex items-center gap-3 overflow-x-auto p-4 scrollbar-hide select-none">
        <button 
            v-for="filter in filters" 
            :key="filter.label"
            @click="setFilter(filter.key)"
            class="relative group flex items-center gap-2.5 px-5 py-2.5 rounded-xl border transition-all duration-300 ease-out whitespace-nowrap"
            :class="[
                modelValue === filter.key 
                    ? `bg-[#2B2B40] ${filter.border} shadow-lg scale-105 ring-1 ring-white/5`
                    : 'bg-[#1E1E2D] border-gray-800 text-gray-500 hover:bg-[#252535] hover:text-gray-300 hover:border-gray-700'
            ]"
        >
            <div 
                v-if="modelValue === filter.key" 
                class="absolute inset-0 rounded-xl opacity-10 transition-opacity"
                :class="filter.bgGlow"
            ></div>

            <span 
                class="text-lg transition-transform duration-300"
                :class="modelValue === filter.key ? 'scale-110' : 'grayscale opacity-70 group-hover:grayscale-0 group-hover:opacity-100'"
            >
                {{ filter.icon }}
            </span>
            
            <span 
                class="text-sm font-bold tracking-wide transition-colors"
                :class="modelValue === filter.key ? filter.textClass : ''"
            >
                {{ filter.label }}
            </span>

            <div 
                v-if="modelValue === filter.key"
                class="absolute bottom-0 left-3 right-3 h-[2px] rounded-full opacity-60 blur-[1px]"
                :class="filter.bgGlow"
            ></div>
        </button>
    </div>
</template>

<style scoped>
/* Scrollbar'ı tamamen gizle */
.scrollbar-hide::-webkit-scrollbar {
    display: none;
}
.scrollbar-hide {
    -ms-overflow-style: none;  /* IE and Edge */
    scrollbar-width: none;  /* Firefox */
}
</style>