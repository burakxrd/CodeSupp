<script setup lang="ts">
import { ref, onMounted, defineAsyncComponent } from 'vue';
import { useToast } from 'vue-toastification';
import api from '../../../services/api';
import type { Purchase } from '../../../types';

// BİLEŞENLER
import PurchaseListTable from '../../purchases/PurchaseListTable.vue';

// Modal'ı Lazy Load yapıyoruz
const PurchaseFormModal = defineAsyncComponent(() => 
  import('../../purchases/PurchaseFormModal.vue')
);

// --- TYPES ---
interface PaginationState {
    currentPage: number;
    totalPages: number;
    totalCount: number;
}

// --- CONSTANTS ---
const PAGE_SIZE = 20;

// --- COMPOSABLES ---
const toast = useToast();

// --- STATE ---
const purchases = ref<Purchase[]>([]);
const isLoading = ref<boolean>(true);
const errorMsg = ref<string | null>(null);

// Pagination State
const pagination = ref<PaginationState>({
    currentPage: 1,
    totalPages: 1,
    totalCount: 0
});

// Modal State
const showModal = ref<boolean>(false);
const selectedPurchase = ref<Purchase | null>(null);

// --- API ACTIONS ---

/**
 * Alım geçmişini API'den çeker.
 * @param {number} page - İstenen sayfa numarası
 */
const fetchData = async (page: number = 1) => {
    isLoading.value = true;
    errorMsg.value = null;
    
    try {
        // API servisinin dönüş tipine göre 'any' kullanıldı ama 
        // items'ın Purchase[] olduğunu garanti ediyoruz.
        const res: any = await api.getPurchaseHistory(page, PAGE_SIZE);
        
        if (res && res.items) {
            purchases.value = res.items as Purchase[];
            pagination.value = {
                // Backend'den dönen alan isimleri önemli (page vs pageNumber)
                currentPage: res.pageNumber || res.page || 1,
                totalPages: res.totalPages || 1,
                totalCount: res.totalCount || res.total || 0
            };
        } else {
            purchases.value = [];
            pagination.value.totalCount = 0;
        }
    } catch (err) {
        console.error("Fetch Error:", err);
        errorMsg.value = "Veriler yüklenirken bir sorun oluştu. Lütfen tekrar deneyin.";
        toast.error("Veri çekme hatası!");
    } finally {
        isLoading.value = false;
    }
};

/**
 * Kayıt silme işlemi
 * @param {number} id - Silinecek kaydın ID'si
 */
const handleDelete = async (id: number) => {
    if (!window.confirm('Bu kaydı silerseniz stoktan düşülecektir. İşlemi onaylıyor musunuz?')) return;

    try {
        await api.deletePurchase(id);
        toast.success("Kayıt başarıyla silindi.");

        // Eğer bulunduğumuz sayfadaki son kaydı sildiysek, bir önceki sayfaya dön
        if (purchases.value.length === 1 && pagination.value.currentPage > 1) {
            await fetchData(pagination.value.currentPage - 1);
        } else {
            await fetchData(pagination.value.currentPage);
        }
    } catch (err) {
        console.error("Delete Error:", err);
        toast.error("Silme işlemi başarısız oldu.");
    }
};

// --- EVENT HANDLERS ---

const onPageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= pagination.value.totalPages) {
        fetchData(newPage);
    }
};

// --- MODAL CONTROLLERS ---

// Parent component (ProductsView) tarafından çağrılır (defineExpose ile)
const openAddModal = () => {
    selectedPurchase.value = null;
    showModal.value = true;
};

// Table component tarafından çağrılır
const openEditModal = (purchase: Purchase) => {
    // Referans koparmak için object spread
    selectedPurchase.value = { ...purchase };
    showModal.value = true;
};

const handleSaveSuccess = () => {
    fetchData(pagination.value.currentPage);
};

// Dışarıya fonksiyon açıyoruz
defineExpose({ openAddModal });

// --- LIFECYCLE ---
onMounted(() => {
    fetchData();
});
</script>

<template>
    <div class="text-gray-300 font-sans h-full flex flex-col">
        
        <transition name="fade">
            <div v-if="errorMsg" class="mb-4 p-4 bg-red-500/10 border border-red-500/20 rounded-xl flex items-center justify-between text-red-400">
                <div class="flex items-center gap-3">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                    <span class="font-medium">{{ errorMsg }}</span>
                </div>
                <button @click="fetchData(pagination.currentPage)" class="text-sm bg-red-500/20 hover:bg-red-500/30 px-3 py-1.5 rounded-lg transition-colors">
                    Tekrar Dene
                </button>
            </div>
        </transition>

        <div class="bg-card rounded-xl shadow-xl border border-card-border/50 overflow-hidden flex flex-col flex-1">
            <PurchaseListTable 
                :purchases="purchases"
                :is-loading="isLoading"
                :total-count="pagination.totalCount"
                :current-page="pagination.currentPage"
                :total-pages="pagination.totalPages"
                :page-size="PAGE_SIZE"
                @page-change="onPageChange"
                @edit="openEditModal"
                @delete="handleDelete"
            />
        </div>

        <PurchaseFormModal 
            v-if="showModal"
            :is-open="showModal" 
            :purchase-to-edit="selectedPurchase"
            @close="showModal = false"
            @save-success="handleSaveSuccess"
        />
    </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>