<script setup lang="ts">
import { computed } from 'vue';

defineOptions({
  inheritAttrs: false
});

// --- TYPES & PROPS ---
interface Props {
  label?: string;
  modelValue?: string | number | null; 
  placeholder?: string;
  rows?: string | number;
  required?: boolean;
  disabled?: boolean;
  error?: string;
  helpText?: string;
  id?: string | null;
}

const props = withDefaults(defineProps<Props>(), {
  label: '',
  modelValue: '', 
  placeholder: '',
  rows: 3,
  required: false,
  disabled: false,
  error: '',
  helpText: '',
  id: null
});

// --- EMITS ---
const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
  (e: 'blur', event: Event): void;
  (e: 'focus', event: Event): void;
}>();

// ... (Geri kalan kod aynı kalacak)
const textareaId = computed<string>(() => props.id || `textarea-${Math.random().toString(36).slice(2, 9)}`);
const helpTextId = computed<string>(() => `${textareaId.value}-help`);
const errorId = computed<string>(() => `${textareaId.value}-error`);

const textareaClasses = computed<string>(() => {
  const base = "w-full bg-[#151521] border rounded-lg px-4 py-3 text-white outline-none transition-all placeholder-gray-600 resize-y min-h-[80px] disabled:opacity-50 disabled:cursor-not-allowed";
  
  let stateClass = "border-gray-700 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500";
  
  if (props.error) {
    stateClass = "border-red-500/50 focus:border-red-500 focus:ring-1 focus:ring-red-500 text-red-100 placeholder-red-300/50";
  }

  return `${base} ${stateClass}`;
});

const handleInput = (event: Event) => {
  const target = event.target as HTMLTextAreaElement;
  emit('update:modelValue', target.value);
};
</script>

<template>
  <div class="w-full">
    <label 
      v-if="label" 
      :for="textareaId" 
      class="block text-gray-400 text-sm mb-2 font-medium cursor-pointer"
    >
      {{ label }} 
      <span v-if="required || $attrs.required" class="text-red-500">*</span>
    </label>
    
    <div class="relative">
      <textarea
        :id="textareaId"
        :value="modelValue"
        :rows="rows"
        :placeholder="placeholder"
        :disabled="disabled"
        :aria-invalid="!!error"
        :aria-describedby="error ? errorId : (helpText ? helpTextId : undefined)"
        @input="handleInput"
        @blur="emit('blur', $event)"
        @focus="emit('focus', $event)"
        :class="textareaClasses"
        v-bind="$attrs"
      ></textarea>
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