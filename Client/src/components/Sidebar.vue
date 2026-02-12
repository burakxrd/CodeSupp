<script setup lang="ts">
import { useRoute } from 'vue-router'; 

const route = useRoute();

// --- TYPES ---
interface MenuItem {
  name: string;
  path: string;
  icon: string;
}

// --- PROPS & EMITS ---
// TypeScript Generic Syntax ile Props ve Defaults
withDefaults(defineProps<{
  isOpen?: boolean
}>(), {
  isOpen: false
});

// TypeScript Generic Syntax ile Emits
const emit = defineEmits<{
  (e: 'close'): void
}>();

// --- MENÜ YAPILANDIRMASI ---
const menuItems: MenuItem[] = [
  { 
    name: 'Ana Panel', 
    path: '/dashboard', 
    icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6' 
  },
  { 
    name: 'Müşteriler', 
    path: '/dashboard/customers', 
    icon: 'M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z' 
  },
  { 
    name: 'Ürünler', 
    path: '/dashboard/products', 
    icon: 'M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4' 
  },
  { 
    name: 'Siparişler', 
    path: '/dashboard/sales', 
    icon: 'M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z' 
  },
  { 
    name: 'Finans Yönetimi', 
    path: '/dashboard/finance', 
    icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z' 
  },
  { 
    name: 'Ayarlar', 
    path: '/dashboard/settings', 
    icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z' 
  }
];

const isActive = (path: string): boolean => {
    if (path === '/dashboard') {
        return route.path === '/dashboard';
    }
    return route.path.startsWith(path);
};

const handleLinkClick = (): void => {
    emit('close');
};
</script>

<template>
  <div 
    v-if="isOpen" 
    @click="$emit('close')" 
    class="fixed inset-0 bg-black/60 backdrop-blur-sm z-40 lg:hidden transition-opacity"
  ></div>

  <aside 
    class="fixed lg:static top-0 left-0 h-full w-72 bg-[#1E1E2D] border-r border-gray-800 flex flex-col shadow-2xl z-50 transition-transform duration-300 ease-in-out lg:transform-none lg:shadow-xl"
    :class="isOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'"
  >
    
    <div class="h-20 flex items-center justify-between px-8 border-b border-gray-800 shrink-0">
        <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-gradient-to-tr from-indigo-500 to-purple-600 flex items-center justify-center shadow-lg shadow-indigo-500/20">
                <span class="text-white font-bold text-lg">C</span>
            </div>
            <span class="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-gray-400 tracking-wide">
                CodeSupp
            </span>
        </div>
        
        <button 
            @click="$emit('close')" 
            class="lg:hidden text-gray-500 hover:text-white transition-colors"
        >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
        </button>
    </div>

    <nav class="flex-1 px-4 py-6 space-y-1.5 overflow-y-auto custom-scrollbar">
      
      <router-link 
        v-for="item in menuItems" 
        :key="item.path" 
        :to="item.path"
        @click="handleLinkClick"
        class="flex items-center gap-3 px-4 py-3.5 rounded-xl transition-all duration-200 group relative font-medium text-sm"
        :class="isActive(item.path) 
            ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/25' 
            : 'text-gray-400 hover:bg-[#2B2B40] hover:text-gray-100'"
      >
        <svg 
            xmlns="http://www.w3.org/2000/svg" 
            class="h-5 w-5 transition-colors"
            :class="isActive(item.path) ? 'text-white' : 'text-gray-500 group-hover:text-white'"
            fill="none" 
            viewBox="0 0 24 24" 
            stroke="currentColor" 
            stroke-width="2"
        >
            <path stroke-linecap="round" stroke-linejoin="round" :d="item.icon" />
        </svg>

        <span>{{ item.name }}</span>

        <div v-if="isActive(item.path)" class="absolute right-4 w-1.5 h-1.5 rounded-full bg-white/50"></div>
      </router-link>

    </nav>

    <div class="p-6 border-t border-gray-800 bg-[#1E1E2D] shrink-0">
        <div class="flex items-center gap-3 p-3 rounded-xl bg-[#151521] border border-gray-800">
            <div class="w-8 h-8 rounded-full bg-gray-700 flex items-center justify-center text-xs text-gray-400">
                v1
            </div>
            <div>
                <p class="text-xs text-gray-400 font-medium">Versiyon 1.0.0</p>
                <p class="text-[10px] text-gray-600">Stable Release</p>
            </div>
        </div>
    </div>

  </aside>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #2B2B40;
  border-radius: 10px;
}
</style>