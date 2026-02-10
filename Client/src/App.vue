<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { useToast } from "vue-toastification";

const toast = useToast();

const handleApiError = (event: Event) => {
  const message = (event as CustomEvent).detail;
  toast.error(message);
};

onMounted(() => {
  window.addEventListener('api-error', handleApiError);
});

onUnmounted(() => {
  window.removeEventListener('api-error', handleApiError);
});
</script>

<template>
  <div class="bg-[#151521] min-h-screen text-gray-300 font-sans">
    <router-view />
  </div>
</template>