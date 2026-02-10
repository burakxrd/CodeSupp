<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '../../services/saleService'; 
import { useDataTable } from '../../composables/useDataTable';
import { SHIPPING_STATUS, PAYMENT_STATUS } from '../../constants/saleStatuses'; // Dosya adını değiştirmeyi unutma!
import { useToast } from "vue-toastification"; 

// --- BİLEŞENLER ---
// [DÜZELTME] Dosya adı SaleListTable.vue (Tekil) olduğu için import da öyle olmalı
import SaleListTable from '../../components/sales/SaleListTable.vue';
import CreateSaleModal from '../../components/sales/CreateSaleModal.vue';
import PaymentModal from '../../components/sales/PaymentModal.vue';

const route = useRoute();
const router = useRouter();
const toast = useToast();

// --- TYPES ---
// --- TYPES ---
interface Sale {
    id: number;
    shippingStatus?: string;      
    rowVersion?: string;
    customerId?: number | null;   
    remainingAmount?: number;     
    [key: string]: any;
}

interface SaleFilters {
    startDate: string;
    endDate: string;
    paymentStatus: string;
    shippingStatus: string;
    sortBy: string;
    sortDir: string;
}

// --- STATE ---
const highlightId = ref<number | undefined>(undefined);
const modals = reactive({ create: false, payment: false });

const customers = ref<any[]>([]);
const products = ref<any[]>([]);
const selectedSaleForPayment = ref<Sale | undefined>(undefined);

// --- [DÜZELTME] TYPESCRIPT HATASI İÇİN LİSTELER ---
// Template içinde Object.values() kullanmak yerine burada tip dönüşümü yapıyoruz.
// Bu sayede :key="status" hatası ortadan kalkar.
const shippingStatusOptions = Object.values(SHIPPING_STATUS) as string[];
const paymentStatusOptions = Object.values(PAYMENT_STATUS) as string[];

// --- FILTERS (Reactive Ref) ---
const filters = ref<SaleFilters>({
    startDate: '',
    endDate: '',
    paymentStatus: '',
    shippingStatus: '',
    sortBy: 'date',
    sortDir: 'desc'
});

// --- COMPOSABLE & ADAPTER ---
const fetchSalesAdapter = async (p: any) => {
    const res: any = await api.getSales(
        p.page,
        p.pageSize,
        p.search,
        p.shippingStatus || null,
        p.paymentStatus || null,
        p.startDate || null,
        p.endDate || null,
        p.sortBy,
        p.sortDir
    );

    return {
        items: res.items || res.data || [],
        totalCount: res.total || res.totalCount || 0,
        totalPages: Math.ceil((res.total || res.totalCount || 0) / p.pageSize),
        pageNumber: res.page || p.page,
        pageSize: res.pageSize || p.pageSize
    };
};

const deleteSaleWrapper = async (id: string | number) => {
    return await api.deleteSale(Number(id));
};

const {
    items: sales,
    isLoading,
    searchQuery,
    currentPage,
    totalPages,
    totalCount,
    pageSize,
    fetchData,
    changePage,
    handleDelete
} = useDataTable<Sale>({
    apiFetch: fetchSalesAdapter,
    apiDelete: deleteSaleWrapper,
    filters: filters,
    pageSizeInit: 10
});

// --- HELPER ACTIONS ---

const handleStatusUpdate = async ({ id, status }: { id: number, status: string }) => {
    const sale = sales.value.find(s => s.id === id);
    if (!sale) return;

    const oldStatus = sale.shippingStatus;

    try {
        const updatedSale = await api.updateShippingStatus(id, status);
        Object.assign(sale, updatedSale);
        toast.success(`Sipariş #${id} durumu güncellendi.`);
    } catch (err) {
        sale.shippingStatus = oldStatus; 
        console.error("Durum güncelleme hatası:", err);
        toast.error("Güncelleme başarısız oldu.");
    }
};

const loadCreateData = async () => {
    try {
        const data: any = await api.getSaleCreateData();
        customers.value = data.availableCustomers || [];
        products.value = data.availableProducts || [];
    } catch (err) {
        console.error("Modal verisi yüklenemedi");
    }
};

const openCreateModal = async () => {
    await loadCreateData();
    modals.create = true;
};

const openPaymentModal = (sale: Sale) => {
    selectedSaleForPayment.value = sale;
    modals.payment = true;
};

const onDeleteSale = async (id: number) => {
    await handleDelete(id, 'Bu siparişi silerseniz stoklar geri yüklenecektir. Emin misiniz?');
};

const clearFilters = () => {
    searchQuery.value = '';
    filters.value = {
        startDate: '',
        endDate: '',
        paymentStatus: '',
        shippingStatus: '',
        sortBy: 'date',
        sortDir: 'desc'
    };
};

// --- INIT ---
onMounted(async () => {
    await fetchData();
    
    if (route.query.highlight) {
        highlightId.value = parseInt(route.query.highlight as string);
        router.replace({ query: {} });
        setTimeout(() => { highlightId.value = undefined; }, 5000);
    }
});
</script>

<template>
    <div class="min-h-screen text-gray-300 font-sans">
        
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
            <div>
                <h1 class="text-2xl font-bold text-white tracking-wide">Sipariş Yönetimi</h1>
                <p class="text-gray-500 text-sm mt-1">Toplam {{ totalCount }} sipariş listeleniyor.</p>
            </div>
            
            <button @click="openCreateModal" class="bg-gradient-to-r from-secondary to-primary hover:from-secondary-hover hover:to-primary-hover text-white px-6 py-3 rounded-xl font-bold text-base shadow-lg shadow-primary/30 flex items-center gap-2 transition-all transform hover:scale-105 active:scale-95">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M10 2a4 4 0 00-4 4v1H5a1 1 0 00-.994.89l-1 9A1 1 0 004 18h12a1 1 0 00.994-1.11l-1-9A1 1 0 0015 7h-1V6a4 4 0 00-4-4zm2 5V6a2 2 0 10-4 0v1h4zm-6 3a1 1 0 112 0 1 1 0 01-2 0zm7-1a1 1 0 100 2 1 1 0 000-2z" clip-rule="evenodd" /></svg>
                Yeni Sipariş Oluştur
            </button>
        </div>

        <div class="bg-card p-5 rounded-xl border border-card-border mb-6 shadow-xl">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                
                <div class="relative">
                    <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500">🔍</span>
                    <input 
                        v-model="searchQuery" 
                        type="text" 
                        placeholder="Müşteri veya Ürün Ara..." 
                        class="w-full bg-input text-gray-300 border border-input-border rounded-lg py-2.5 pl-10 pr-4 focus:border-primary outline-none text-sm transition-colors"
                    >
                </div>

                <div class="flex gap-2">
                    <input type="date" v-model="filters.startDate" class="w-1/2 bg-input text-gray-400 border border-input-border rounded-lg py-2 px-3 focus:border-primary outline-none text-sm [color-scheme:dark]">
                    <input type="date" v-model="filters.endDate" class="w-1/2 bg-input text-gray-400 border border-input-border rounded-lg py-2 px-3 focus:border-primary outline-none text-sm [color-scheme:dark]">
                </div>

                <select v-model="filters.shippingStatus" class="bg-input text-gray-300 border border-input-border rounded-lg py-2.5 px-3 focus:border-primary outline-none text-sm appearance-none">
                    <option value="">Tüm Kargo Durumları</option>
                    <option v-for="status in shippingStatusOptions" :key="status" :value="status">
                        {{ status }}
                    </option>
                </select>

                <select v-model="filters.paymentStatus" class="bg-input text-gray-300 border border-input-border rounded-lg py-2.5 px-3 focus:border-primary outline-none text-sm appearance-none">
                    <option value="">Tüm Ödemeler</option>
                    <option v-for="status in paymentStatusOptions" :key="status" :value="status">
                        {{ status }}
                    </option>
                </select>
            </div>

            <div class="flex flex-col md:flex-row justify-between items-center mt-4 pt-4 border-t border-card-border gap-4">
                <div class="flex items-center gap-2 w-full md:w-auto">
                    <span class="text-sm text-gray-500 whitespace-nowrap">Sırala:</span>
                    <select v-model="filters.sortBy" class="bg-input text-gray-300 border border-input-border rounded-lg py-1.5 px-3 text-sm focus:border-primary outline-none">
                        <option value="date">Tarih</option>
                        <option value="total">Tutar</option>
                        <option value="collected">Tahsil Edilen</option>
                        <option value="remaining">Kalan</option>
                    </select>
                    <select v-model="filters.sortDir" class="bg-input text-gray-300 border border-input-border rounded-lg py-1.5 px-3 text-sm focus:border-primary outline-none">
                        <option value="desc">Azalan</option>
                        <option value="asc">Artan</option>
                    </select>
                </div>
                
                <button @click="clearFilters" class="text-sm text-red-400 hover:text-white transition-colors flex items-center gap-1">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" /></svg>
                    Filtreleri Temizle
                </button>
            </div>
        </div>

        <SaleListTable 
            :sales="sales"
            :is-loading="isLoading"
            :highlight-id="highlightId"
            :total-count="totalCount"
            :current-page="currentPage"
            :total-pages="totalPages"
            :page-size="pageSize"
            @page-change="changePage"
            @pay="openPaymentModal"
            @delete="onDeleteSale"
            @update-status="handleStatusUpdate"
        />

        <CreateSaleModal 
            :is-open="modals.create"
            :customers="customers"
            :products="products"
            @close="modals.create = false"
            @sale-created="fetchData(1)"
        />

        <PaymentModal 
            :is-open="modals.payment"
            :sale="selectedSaleForPayment"
            @close="modals.payment = false"
            @payment-success="fetchData(currentPage)"
        />

    </div>
</template>