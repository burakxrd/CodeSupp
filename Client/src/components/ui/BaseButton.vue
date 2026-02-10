<script setup lang="ts">
import { computed, ref } from 'vue';

// --- TYPES ---
type ButtonType = 'button' | 'submit' | 'reset';
type ButtonVariant = 'primary' | 'secondary' | 'success' | 'danger' | 'outline';

interface Props {
  type?: ButtonType;
  loading?: boolean;
  disabled?: boolean;
  block?: boolean;
  variant?: ButtonVariant;
}

// --- PROPS ---
const props = withDefaults(defineProps<Props>(), {
  type: 'button',
  loading: false,
  disabled: false,
  block: false,
  variant: 'primary'
});

const buttonRef = ref<HTMLElement | null>(null);

// Temel Sınıflar
const baseClasses: string[] = [
  "relative", 
  "rounded-xl font-bold transition-all duration-300", 
  "flex items-center justify-center gap-2",
  "active:scale-95",
  "disabled:opacity-50 disabled:cursor-not-allowed disabled:active:scale-100",
  "no-underline"
];

// Renk Haritası
const variants: Record<ButtonVariant, string> = {
  primary: "bg-gradient-to-r from-secondary to-primary hover:from-secondary-hover hover:to-primary-hover text-white shadow-lg shadow-primary/30 border-none hover:scale-[1.02]",
  success: "bg-gradient-to-r from-emerald-500 to-teal-500 hover:from-emerald-400 hover:to-teal-400 text-white shadow-lg shadow-emerald-500/30 hover:scale-[1.02]",
  danger: "bg-red-500/10 hover:bg-red-600 text-red-400 hover:text-white border border-red-500/20 hover:border-transparent",
  secondary: "text-gray-400 hover:text-white hover:bg-white/5 border border-transparent hover:border-gray-700",
  outline: "border border-gray-600 text-gray-300 hover:border-gray-400 hover:text-white"
};

// Boyut Haritası (Variant bazlı özelleştirme için)
const sizes: Record<string, string> = {
  default: "px-6 py-3",
  danger: "px-4 py-2.5", 
  outline: "px-4 py-2"
};

const computedClass = computed<string>(() => {
  const variantClass = variants[props.variant] || variants.primary;
  
  // Boyut mantığı: Eğer variant danger veya outline ise özel boyut, değilse default
  const sizeKey = (props.variant === 'danger' || props.variant === 'outline') ? props.variant : 'default';
  const sizeClass = sizes[sizeKey] || sizes.default;

  return [
    ...baseClasses,
    variantClass,
    sizeClass,
    props.block ? 'w-full' : ''
  ].join(' ');
});

defineExpose({
  focus: () => buttonRef.value?.focus(),
  el: buttonRef
});
</script>

<template>
  <component
    :is="$attrs.to ? 'router-link' : 'button'"
    ref="buttonRef"
    :type="$attrs.to ? undefined : type"
    :class="computedClass"
    :disabled="disabled || loading"
    :aria-busy="loading"
    :aria-disabled="disabled || loading"
    v-bind="$attrs"
  >
    <span 
      v-if="loading" 
      class="absolute inset-0 flex items-center justify-center"
    >
      <svg class="animate-spin h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
      </svg>
    </span>

    <span 
      :class="{ 'opacity-0': loading, 'invisible': loading }"
      class="flex items-center gap-2"
    >
      <slot />
    </span>
  </component>
</template>