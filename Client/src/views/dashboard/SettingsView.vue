<script setup lang="ts">
import { ref, onMounted } from 'vue'; 
import { useSettingsStore } from '../../stores/settings';
import CategoryManager from '../../components/settings/CategoryManager.vue'; 

const settingsStore = useSettingsStore();

// --- TYPES ---
type TabId = 'general' | 'categories';

interface Tab {
    id: TabId;
    label: string;
    icon: string;
}

// Aktif Sekme (Tabs) Yönetimi
const activeTab = ref<TabId>('general'); 

// Sayfa yüklendiğinde ayarları API'den çek
onMounted(async () => {
    await settingsStore.fetchSettings();
});

// Sekme Tanımları
const tabs: Tab[] = [
    { id: 'general', label: 'Genel Ayarlar', icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z' },
    { id: 'categories', label: 'Kategori Yönetimi', icon: 'M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z' }
];
</script>

<template>
    <div class="min-h-screen text-gray-300">
        <div class="mb-8">
            <h1 class="text-2xl font-bold text-white tracking-wide">Mağaza Ayarları</h1>
            <p class="text-gray-500 text-sm mt-1">Panel görünümünü ve sistem ayarlarını buradan yönetebilirsiniz.</p>
        </div>

        <div class="flex flex-col lg:flex-row gap-6">
            <div class="lg:w-64 flex-shrink-0">
                <nav class="flex lg:flex-col gap-2 overflow-x-auto lg:overflow-visible pb-2 lg:pb-0">
                    <button 
                        v-for="tab in tabs" 
                        :key="tab.id"
                        @click="activeTab = tab.id"
                        :class="[
                            'flex items-center gap-3 px-4 py-3 text-sm font-medium rounded-lg transition-colors whitespace-nowrap',
                            activeTab === tab.id 
                                ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/30' 
                                : 'bg-[#1E1E2D] text-gray-400 hover:bg-[#2A2A3C] hover:text-white'
                        ]"
                    >
                        <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="tab.icon" />
                        </svg>
                        {{ tab.label }}
                    </button>
                </nav>
            </div>

            <div class="flex-1">
                <div v-if="settingsStore.isLoading" class="bg-[#1E1E2D] rounded-xl p-10 text-center text-gray-500">
                    Ayarlar yükleniyor...
                </div>

                <div v-else class="bg-[#1E1E2D] rounded-xl shadow-xl border border-gray-800/50 p-6 min-h-[400px]">
                    
                    <div v-if="activeTab === 'general'">
                        <h3 class="text-lg font-bold text-white mb-6 border-b border-gray-700 pb-4">Genel Görünüm Ayarları</h3>

                        <div class="space-y-6">
                            <div class="flex items-center justify-between py-2">
                                <div class="pr-4">
                                    <h4 class="text-white font-medium text-base">Kargo Durumunu Göster</h4>
                                    <p class="text-gray-500 text-sm mt-1">
                                        Sipariş listesinde "Kargo Durumu" (Sipariş Alındı, Kargolandı vb.) sütununu aktif eder.
                                    </p>
                                </div>
                                <button 
                                    @click="settingsStore.toggleShippingColumn()"
                                    :class="['relative inline-flex h-7 w-12 items-center rounded-full transition-colors focus:outline-none', settingsStore.showShippingColumn ? 'bg-indigo-600' : 'bg-gray-700']"
                                >
                                    <span :class="['inline-block h-5 w-5 transform rounded-full bg-white transition-transform shadow-md', settingsStore.showShippingColumn ? 'translate-x-6' : 'translate-x-1']" />
                                </button>
                            </div>
                        </div>

                        <h3 class="text-lg font-bold text-white mt-10 mb-6 border-b border-gray-700 pb-4">Finansal Ayarlar</h3>
                        
                        <div class="space-y-6">
                            <div class="flex items-center justify-between py-2">
                                <div class="pr-4">
                                    <h4 class="text-white font-medium text-base">Vergi / KDV Hesaplaması</h4>
                                    <p class="text-gray-500 text-sm mt-1">
                                        Sipariş oluşturma ekranında Vergi/KDV hesaplama alanlarını aktif eder.
                                    </p>
                                </div>
                                <button 
                                    @click="settingsStore.toggleVAT()"
                                    :class="['relative inline-flex h-7 w-12 items-center rounded-full transition-colors focus:outline-none', settingsStore.enableVAT ? 'bg-indigo-600' : 'bg-gray-700']"
                                >
                                    <span :class="['inline-block h-5 w-5 transform rounded-full bg-white transition-transform shadow-md', settingsStore.enableVAT ? 'translate-x-6' : 'translate-x-1']" />
                                </button>
                            </div>

                            <div v-if="settingsStore.enableVAT" class="bg-[#151521] border border-gray-700 rounded-lg p-4 flex items-center justify-between animate-fade-in">
                                <div>
                                    <h4 class="text-white font-medium text-sm">Varsayılan KDV Oranı (%)</h4>
                                    <p class="text-gray-500 text-xs mt-1">Yeni siparişlerde otomatik gelecek oran.</p>
                                </div>
                                <div class="relative w-24">
                                    <input 
                                        :value="settingsStore.defaultVAT"
                                        @change="(e) => settingsStore.updateDefaultVAT(Number((e.target as HTMLInputElement).value))"
                                        type="number" 
                                        min="0" 
                                        max="100"
                                        class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 py-2 text-white text-sm text-center focus:border-indigo-500 outline-none transition-colors"
                                    >
                                    <span class="absolute right-7 top-1/2 -translate-y-1/2 text-gray-500 text-xs font-bold pointer-events-none">%</span>
                                </div>
                            </div>
                        </div>

                    </div>

                    <div v-if="activeTab === 'categories'">
                        <CategoryManager />
                    </div>

                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
@keyframes fadeIn {
    from { opacity: 0; transform: translateY(-5px); }
    to { opacity: 1; transform: translateY(0); }
}
.animate-fade-in { animation: fadeIn 0.3s ease-out; }
</style>