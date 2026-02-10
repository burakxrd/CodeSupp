<script setup lang="ts">
import { computed } from 'vue';

// --- PROPS ---
interface Props {
  name?: string;      // Örn: "Cem Sayar"
  size?: 'sm' | 'md' | 'lg';
  variant?: 'square' | 'circle';
  image?: string;     // Profil resmi varsa
}

const props = withDefaults(defineProps<Props>(), {
  name: 'System Admin',
  size: 'md',
  variant: 'square',
  image: undefined
});

// --- COMPUTED: Baş Harfleri Çıkar ---
// "Cem Sayar" -> "CS", "Ali" -> "A", "Veli Ali Kırkdokuzelli" -> "VA"
const initials = computed(() => {
  if (!props.name) return '??';
  const parts = props.name.trim().split(' ');
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
});

// --- COMPUTED: İsme Göre Sabit Renk (Deterministic Hashing) ---
// Bu fonksiyon aynı isim için her zaman aynı renk kodunu üretir.
const dynamicColor = computed(() => {
  let hash = 0;
  for (let i = 0; i < props.name.length; i++) {
    hash = props.name.charCodeAt(i) + ((hash << 5) - hash);
  }
  
  // HSL kullanarak pastel tonlar üretelim (Saturation: 70%, Lightness: 50%)
  const h = Math.abs(hash) % 360;
  return `hsl(${h}, 70%, 50%)`; 
});

// --- DYNAMIC STYLES ---
const sizeClasses = {
  sm: 'w-8 h-8 text-xs',
  md: 'w-12 h-12 text-xl',
  lg: 'w-16 h-16 text-2xl'
};

const radiusClass = props.variant === 'circle' ? 'rounded-full' : 'rounded-xl';
</script>

<template>
  <div 
    class="flex items-center justify-center shadow-lg transition-transform hover:scale-105 select-none overflow-hidden"
    :class="[sizeClasses[size], radiusClass]"
    :style="{ backgroundColor: image ? 'transparent' : dynamicColor }"
  >
    <img 
      v-if="image" 
      :src="image" 
      :alt="name" 
      class="w-full h-full object-cover"
    />
    
    <span v-else class="text-white font-bold tracking-tighter font-mono drop-shadow-md">
      {{ initials }}
    </span>
  </div>
</template>