<script setup lang="ts">
import { computed, useAttrs } from 'vue';

defineOptions({
  inheritAttrs: false
});

// --- TYPES & PROPS ---
interface Props {
  label?: string;
  modelValue?: string | number | null; 
  id?: string | null;
  placeholder?: string;
  error?: string;
  helpText?: string;
  success?: boolean;
  disabled?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  label: '',
  // Varsayılan değer boş string kalsın, null gelirse input boş görünür
  modelValue: '', 
  id: null,
  placeholder: '',
  error: '',
  helpText: '',
  success: false,
  disabled: false
});

// --- EMITS ---
const emit = defineEmits<{
  // Inputtan çıkan değer genellikle string veya number olur, null olmaz.
  // O yüzden burayı değiştirmemize gerek yok, ama istersen | null ekleyebilirsin.
  (e: 'update:modelValue', value: string | number): void; 
  (e: 'blur', event: Event): void;
  (e: 'focus', event: Event): void;
}>();

const attrs = useAttrs();

// Benzersiz ID oluşturma
const inputId = computed<string>(() => props.id || `input-${Math.random().toString(36).slice(2, 9)}`);
const helpTextId = computed<string>(() => `${inputId.value}-help`);
const errorId = computed<string>(() => `${inputId.value}-error`);

// Input sınıflarını dinamik hesapla
const inputClasses = computed<string>(() => {
  const base = "w-full bg-[#151521] border rounded-lg py-3 text-white outline-none transition-all placeholder-gray-600 disabled:opacity-50 disabled:cursor-not-allowed";
  
  let stateClass = "border-gray-700 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500";
  
  if (props.error) {
    stateClass = "border-red-500/50 focus:border-red-500 focus:ring-1 focus:ring-red-500 text-red-100";
  } else if (props.success) {
    stateClass = "border-green-500/50 focus:border-green-500 focus:ring-1 focus:ring-green-500";
  }

  return `${base} ${stateClass} px-4`;
});

const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement;
  emit('update:modelValue', target.value);
};
</script>

<template>
  <div class="w-full">
    <label 
      v-if="label" 
      :for="inputId" 
      class="block text-gray-400 text-sm mb-2 font-medium cursor-pointer"
    >
      {{ label }} 
      <span v-if="$attrs.required" class="text-red-500">*</span>
    </label>
    
    <div class="relative">
      <div v-if="$slots.prepend" class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-gray-500">
        <slot name="prepend" />
      </div>

      <input
        :id="inputId"
        :value="modelValue"
        v-bind="$attrs" 
        :disabled="disabled"
        :placeholder="placeholder"
        :aria-invalid="!!error"
        :aria-describedby="error ? errorId : (helpText ? helpTextId : undefined)"
        @input="handleInput"
        @blur="emit('blur', $event)"
        @focus="emit('focus', $event)"
        :class="[
          inputClasses,
          $slots.prepend ? 'pl-10' : '',
          $slots.append ? 'pr-10' : '' 
        ]"
      >

      <div v-if="$slots.append" class="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-500">
        <slot name="append" />
      </div>
    </div>
    
    <transition name="slide-fade">
      <p v-if="error" :id="errorId" class="text-red-400 text-xs mt-1 flex items-center gap-1">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-3 w-3" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
        </svg>
        {{ error }}
      </p>
    </transition>

    <p v-if="helpText && !error" :id="helpTextId" class="text-gray-500 text-xs mt-1">
      {{ helpText }}
    </p>
  </div>
</template>

<style scoped>
.slide-fade-enter-active,
.slide-fade-leave-active {
  transition: all 0.2s ease-out;
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateY(-5px);
  opacity: 0;
}
</style>