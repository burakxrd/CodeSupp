<script setup lang="ts">
import { computed, useAttrs } from 'vue';

// Attribute Inheritance'ı kapatıyoruz.
defineOptions({
  inheritAttrs: false
});

// --- TYPES & PROPS ---
type ModelValue = string | number | boolean | Record<string, any>;

interface Props {
  label?: string;
  modelValue?: ModelValue;
  options: any[]; // Seçenekler obje veya primitif olabilir
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
  loading?: boolean;
  error?: string;
  helpText?: string;
  id?: string | null;
  valueKey?: string;
  labelKey?: string;
}

const props = withDefaults(defineProps<Props>(), {
  label: '',
  modelValue: '',
  placeholder: 'Seçiniz',
  required: false,
  disabled: false,
  loading: false,
  error: '',
  helpText: '',
  id: null,
  valueKey: 'id',
  labelKey: 'name'
});

// --- EMITS ---
const emit = defineEmits<{
  (e: 'update:modelValue', value: ModelValue): void;
  (e: 'change', value: ModelValue): void;
  (e: 'blur', event: Event): void;
}>();

const attrs = useAttrs();

// ID Yönetimi (A11y)
const selectId = computed<string>(() => props.id || `select-${Math.random().toString(36).slice(2, 9)}`);
const helpTextId = computed<string>(() => `${selectId.value}-help`);
const errorId = computed<string>(() => `${selectId.value}-error`);

// Proxy Model (Çift Yönlü Bağlama & Tip Koruması)
// event.target.value ile uğraşmak yerine Vue'nun v-model'ini kullanıyoruz.
const proxyModel = computed({
  get: () => props.modelValue,
  set: (val) => {
    emit('update:modelValue', val);
    emit('change', val);
  }
});

// Styling Logic
const selectClasses = computed<string>(() => {
  const base = "w-full bg-[#151521] border rounded-lg pl-4 pr-10 py-3 outline-none transition-all appearance-none cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed";
  
  let stateClass = "border-gray-700 focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500";
  
  if (props.error) {
    stateClass = "border-red-500/50 focus:border-red-500 focus:ring-1 focus:ring-red-500 text-red-100";
  }

  // Placeholder rengi kontrolü (Değer boşsa gri, doluysa beyaz)
  const textClass = props.modelValue === '' ? 'text-gray-500' : 'text-white';

  return `${base} ${stateClass} ${textClass}`;
});
</script>

<template>
  <div class="w-full">
    <label 
      v-if="label" 
      :for="selectId" 
      class="block text-gray-400 text-sm mb-2 font-medium cursor-pointer"
    >
      {{ label }} 
      <span v-if="required || $attrs.required" class="text-red-500">*</span>
    </label>
    
    <div class="relative">
      <select
        :id="selectId"
        v-model="proxyModel"
        v-bind="$attrs"
        :disabled="disabled || loading"
        :class="selectClasses"
        :aria-invalid="!!error"
        :aria-describedby="error ? errorId : (helpText ? helpTextId : undefined)"
        @blur="emit('blur', $event)"
      >
        <option value="" disabled>{{ placeholder }}</option>
        
        <option 
          v-for="(option, index) in options" 
          :key="index"
          :value="typeof option === 'object' ? option[valueKey] : option"
        >
          {{ typeof option === 'object' ? option[labelKey] : option }}
        </option>
      </select>

      <div class="absolute inset-y-0 right-0 flex items-center px-3 pointer-events-none text-gray-500">
        
        <svg v-if="loading" class="animate-spin h-4 w-4 text-indigo-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>

        <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
        </svg>
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