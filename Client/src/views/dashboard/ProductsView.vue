<script setup lang="ts">
import { ref, defineAsyncComponent, nextTick } from 'vue';
import BaseButton from '../../components/ui/BaseButton.vue';

// --- TYPES ---
// Child componentlerin (Tab'ların) dışarıya açtığı metodları tanımlıyoruz
interface TabComponentInstance {
    openAddModal: () => void;
}

type TabType = 'products' | 'purchases';

// TAB BİLEŞENLERİ (Lazy Load)
const ProductListTab = defineAsyncComponent(() => 
  import('../../components/products/tabs/ProductListTab.vue')
);

const PurchaseListTab = defineAsyncComponent(() => 
  import('../../components/products/tabs/PurchaseListTab.vue')
);

// STATE
const activeTab = ref<TabType>('products'); // Varsayılan: Ürünler

// TEK REFERANS (O an ekranda hangi tab varsa onu tutar)
const currentTabRef = ref<TabComponentInstance | null>(null);

// SEKME DEĞİŞTİRME
const setTab = (tabName: TabType) => {
    activeTab.value = tabName;
};

// --- AKSİYONLAR ---

// 1. Yeni Ürün Ekle Butonu
const triggerAddProduct = async () => {
    // Eğer başka sekmedeysek önce 'products' sekmesine geç
    if (activeTab.value !== 'products') {
        activeTab.value = 'products';
        // DOM'un güncellenmesini ve componentin yüklenmesini bekle
        await nextTick();
    }
    
    // Küçük bir gecikme ekleyip fonksiyonu çağırıyoruz (Async component yüklenme payı)
    setTimeout(() => {
        if (currentTabRef.value && typeof currentTabRef.value.openAddModal === 'function') {
            currentTabRef.value.openAddModal();
        } else {
            console.error("ProductListTab 'openAddModal' fonksiyonu bulunamadı! defineExpose ekledin mi?");
        }
    }, 50);
};

// 2. Yeni Alım Gir Butonu
const triggerAddPurchase = async () => {
    // Eğer başka sekmedeysek önce 'purchases' sekmesine geç
    if (activeTab.value !== 'purchases') {
        activeTab.value = 'purchases';
        // DOM'un güncellenmesini bekle
        await nextTick();
    }

    // Fonksiyonu çağır
    setTimeout(() => {
        if (currentTabRef.value && typeof currentTabRef.value.openAddModal === 'function') {
            currentTabRef.value.openAddModal();
        } else {
            console.error("PurchaseListTab 'openAddModal' fonksiyonu bulunamadı! defineExpose ekledin mi?");
        }
    }, 50);
};
</script>

<template>
    <div class="min-h-screen font-sans text-gray-300">
        
        <div class="flex flex-col md:flex-row md:items-end justify-between gap-4 mb-8">
            
            <div class="flex flex-col gap-6">
                <div>
                    <h1 class="text-2xl font-bold text-white tracking-wide">Ürün ve Stok Yönetimi</h1>
                    <p class="text-gray-500 text-sm mt-1">
                        Mevcut ürünlerinizi listeleyebilir veya yeni mal alım hareketlerini buradan yönetebilirsiniz.
                    </p>
                </div>

                <div class="flex p-1 bg-[#151521] rounded-xl border border-gray-700/50 w-fit shadow-md">
                    <button 
                        @click="setTab('products')"
                        class="px-5 py-2 rounded-lg text-sm font-bold transition-all duration-300 flex items-center gap-2 outline-none focus:ring-2 focus:ring-indigo-500/50"
                        :class="activeTab === 'products' 
                            ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-500/20' 
                            : 'text-gray-400 hover:text-white hover:bg-white/5'"
                    >
                        <span>📦</span> Ürün Listesi
                    </button>

                    <button 
                        @click="setTab('purchases')"
                        class="px-5 py-2 rounded-lg text-sm font-bold transition-all duration-300 flex items-center gap-2 outline-none focus:ring-2 focus:ring-emerald-500/50"
                        :class="activeTab === 'purchases' 
                            ? 'bg-emerald-600 text-white shadow-lg shadow-emerald-500/20' 
                            : 'text-gray-400 hover:text-white hover:bg-white/5'"
                    >
                        <span>📋</span> Alım Geçmişi
                    </button>
                </div>
            </div>

            <div class="flex items-center gap-3">
                <BaseButton 
                    variant="success" 
                    @click="triggerAddPurchase"
                >
                    <span>+</span> Yeni Alım Gir
                </BaseButton>

                <BaseButton 
                    variant="primary" 
                    @click="triggerAddProduct"
                >
                    <span>+</span> Yeni Ürün Ekle
                </BaseButton>
            </div>

        </div>

        <div class="animate-fade-in-up">
            <KeepAlive>
                <component 
                    :is="activeTab === 'products' ? ProductListTab : PurchaseListTab"
                    ref="currentTabRef"
                />
            </KeepAlive>
        </div>

    </div>
</template>

<style scoped>
.animate-fade-in-up {
    animation: fadeInUp 0.4s ease-out;
}

@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}
</style>