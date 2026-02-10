<script setup lang="ts">
import { ref } from 'vue';
import { useRoute } from 'vue-router';
import Sidebar from '../components/Sidebar.vue';
import DashboardHeader from '../components/DashboardHeader.vue';

const route = useRoute();

// --- STATE ---
// Sidebar'ın açık/kapalı durumunu burada tutuyoruz.
// Varsayılan olarak false (Kapalı/Mobil). Desktop'ta CSS zaten onu görünür kılıyor.
const isSidebarOpen = ref<boolean>(false);

// --- ACTIONS ---
const toggleSidebar = (): void => {
    isSidebarOpen.value = !isSidebarOpen.value;
};

const closeSidebar = (): void => {
    isSidebarOpen.value = false;
};
</script>

<template>
  <div class="flex h-screen bg-[#151521] text-gray-200 font-sans overflow-hidden">
    
    <Sidebar 
        :isOpen="isSidebarOpen" 
        @close="closeSidebar" 
    />

    <div class="flex-1 flex flex-col min-w-0 relative">
      
      <DashboardHeader 
        @toggle-sidebar="toggleSidebar" 
      />

      <main class="flex-1 overflow-y-auto overflow-x-hidden p-4 md:p-8 custom-scrollbar relative">
        
        <router-view v-slot="{ Component }">
          <transition name="fade-slide-up" mode="out-in">
            <component :is="Component" :key="route.fullPath" />
          </transition>
        </router-view>

      </main>
    </div>
  </div>
</template>

<style scoped>
/* --- SAYFA GEÇİŞ ANİMASYONU --- */
.fade-slide-up-enter-active,
.fade-slide-up-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.fade-slide-up-enter-from {
  opacity: 0;
  transform: translateY(10px); /* Aşağıdan yukarı giriş */
}

.fade-slide-up-leave-to {
  opacity: 0;
  transform: translateY(-10px); /* Yukarı doğru çıkış */
}

/* --- MODERN SCROLLBAR (Webkit - Chrome, Safari, Edge) --- */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #2B2B40; 
  border-radius: 20px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background-color: #3699FF; 
}

/* Firefox için Scrollbar */
.custom-scrollbar {
  scrollbar-width: thin;
  scrollbar-color: #2B2B40 transparent;
}
</style>