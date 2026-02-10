<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth'; 
import { useToast } from "vue-toastification";

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const toast = useToast();


const emit = defineEmits<{
  (e: 'toggle-sidebar'): void
}>();

// --- COMPUTED ---
const pageTitles: Record<string, string> = {
  'dashboard-home': 'Genel Bakış',
  'products': 'Ürün Kataloğu',
  'purchases': 'Ürün Alımları',
  'customers': 'Müşteri Yönetimi',
  'orders': 'Sipariş Takibi',
  'finance': 'Finansal Raporlar',
  'ai-center': 'AI Merkezi',
  'settings': 'Ayarlar'
};

const pageTitle = computed<string>(() => {
  // route.name 'string | symbol | undefined' olabilir.
  // String() ile güvenli hale getiriyoruz.
  const routeName = String(route.name || '');
  return pageTitles[routeName] || 'Yönetim Paneli';
});

// İsim Soyisimden Baş Harfleri Çıkar (Ahmet Yılmaz -> AY)
const userInitials = computed<string>(() => {
  const name = authStore.user?.fullName || 'Misafir';
  return name
    .split(' ')       // Boşluktan böl
    .map(n => n[0])   // İlk harfleri al
    .join('')         // Birleştir
    .toUpperCase()    // Büyüt
    .slice(0, 2);     // İlk 2 harfi al
});

// --- ACTIONS ---

const handleLogout = (): void => {
  authStore.logout(); // Token'ı siler
  toast.info("Oturum güvenli bir şekilde kapatıldı.");
  router.push('/login'); 
};
</script>

<template>
  <header class="h-20 bg-[#151521]/90 backdrop-blur-sm border-b border-gray-800 flex items-center justify-between px-4 md:px-8 sticky top-0 z-30 transition-all">
    
    <div class="flex items-center gap-4">
        
        <button 
            @click="emit('toggle-sidebar')" 
            class="p-2 rounded-lg text-gray-400 hover:text-white hover:bg-gray-800 transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-500/50"
        >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h7" />
            </svg>
        </button>

        <div class="flex flex-col">
            <h2 class="text-xl md:text-2xl font-bold text-white tracking-tight leading-none mb-1">
              {{ pageTitle }}
            </h2>
            <div class="hidden md:flex items-center gap-2 text-xs font-medium text-gray-500">
                <span>CodeSupp</span>
                <span class="text-gray-700">/</span>
                <span class="text-indigo-400">{{ pageTitle }}</span>
            </div>
        </div>
    </div>

    <div class="flex items-center gap-3 md:gap-6">
      
        <button class="relative p-2 text-gray-400 hover:text-white transition-colors rounded-lg hover:bg-gray-800/50 group">
            <span class="absolute top-2 right-2 w-2 h-2 bg-red-500 rounded-full ring-2 ring-[#151521] animate-pulse"></span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
            </svg>
        </button>

        <div class="h-8 w-px bg-gray-800 hidden md:block"></div>

        <div class="flex items-center gap-3 md:gap-4">
            
            <div class="text-right hidden md:block">
                <div class="text-sm font-bold text-white">
                    {{ authStore.user?.fullName || 'Misafir Kullanıcı' }}
                </div>
                <div class="text-xs text-gray-500 font-medium">Yönetici</div>
            </div>

            <div class="w-10 h-10 rounded-xl bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white font-bold text-sm shadow-lg shadow-indigo-500/20 border border-white/10 select-none">
                {{ userInitials }}
            </div>

            <button 
                @click="handleLogout" 
                class="bg-gray-800 hover:bg-red-500/10 text-gray-400 hover:text-red-400 p-2.5 rounded-lg transition-all border border-gray-700 hover:border-red-500/20 group"
                title="Güvenli Çıkış"
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 transform group-hover:translate-x-0.5 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
            </button>
        </div>

    </div>
  </header>
</template>