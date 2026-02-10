<script setup lang="ts">
import { ref, onMounted, onActivated, defineAsyncComponent } from 'vue'; 
import { useRoute } from 'vue-router';
import api from '../../../services/api';
import { useToast } from 'vue-toastification';
import { getImageUrl } from '../../../utils/urlHelper';
import { useDataTable } from '../../../composables/useDataTable';
import type { Product, Category } from '../../../types';
import ProductTable from '../ProductTable.vue';
import ImageLightbox from '../../common/ImageLightbox.vue';
const ProductFormModal = defineAsyncComponent(() => 
  import('../ProductFormModal.vue')
);
const toast = useToast();
const route = useRoute();

// --- STATE & FILTERS ---
const filters = ref<{ category: number | null; sort: string }>({
    category: null,
    sort: 'newest'
});

// --- ADAPTER ---
const fetchProductsAdapter = async (p: any) => {
    const res: any = await api.getProducts(
        p.page,
        p.pageSize,
        p.search,
        p.category, 
        p.sort
    );

    return {
        items: res.items,
        totalCount: res.total || res.totalCount || 0, 
        pageNumber: res.page || res.pageNumber || 1, 
        pageSize: p.pageSize,
        totalPages: res.totalPages || 1,
        total: res.total || res.totalCount || 0,
        page: res.page || res.pageNumber || 1
    };
};

// --- LOGIC ---
const { 
    items: products, 
    isLoading, 
    searchQuery,
    currentPage, 
    totalPages, 
    totalCount, 
    pageSize,
    fetchData, 
    changePage, 
    handleDelete: deleteProductProxy 
} = useDataTable<Product>({
    apiFetch: fetchProductsAdapter,
    apiDelete: (id) => api.deleteProduct(Number(id)), 
    filters: filters, 
    pageSizeInit: 10
});

let searchTimeout: ReturnType<typeof setTimeout>;
const handleSearch = (e: Event) => {
    const val = (e.target as HTMLInputElement).value;
    searchQuery.value = val;
    
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        fetchData(); 
    }, 300); 
};

// MODAL & UI STATES
const showModal = ref<boolean>(false);
const isEditing = ref<boolean>(false);
const selectedProduct = ref<Product | null>(null);
const showLightbox = ref<boolean>(false);
const lightboxImage = ref<string>('');
const categories = ref<Category[]>([]);

const loadCategories = async () => {
    try {
        const res: any = await api.getCategories();
        categories.value = Array.isArray(res) ? res : (res.data || []); 
    } catch (err) {
        console.error(err);
        toast.error("Kategori listesi alınamadı.");
    }
};

const openLightbox = (path: string) => {
    if (!path) return;
    const url = getImageUrl(path);
    if (url) {
        lightboxImage.value = url;
        showLightbox.value = true;
    }
};

const openAddModal = () => {
    isEditing.value = false;
    selectedProduct.value = null;
    showModal.value = true;
};

defineExpose({ openAddModal });

const openEditModal = (product: Product) => {
    isEditing.value = true;
    selectedProduct.value = { ...product };
    showModal.value = true;
};

const handleSave = async () => {
    await fetchData(currentPage.value);
    showModal.value = false; 
};

const handleDeleteWrapper = (id: number) => {
    deleteProductProxy(id);
};

// --- LIFECYCLE ---
onMounted(() => {
    if (route.query.sort) {
        filters.value.sort = route.query.sort as string;
    }
    fetchData();
    loadCategories(); 
});

onActivated(() => {
    fetchData(currentPage.value);
});
</script>

<template>
    <div class="text-gray-300 font-sans">
        
        <div class="bg-card rounded-xl shadow-xl border border-card-border/50 overflow-hidden flex flex-col">
            
            <div class="p-5 border-b border-card-border flex flex-col md:flex-row gap-4">
                
                <div class="relative w-full md:flex-1">
                    <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500">🔍</span>
                    <input 
                        :value="searchQuery" 
                        @input="handleSearch" 
                        type="text" 
                        placeholder="Ürün adı, kodu veya kategori..." 
                        class="w-full bg-input text-gray-300 border border-input-border rounded-lg py-3 pl-10 pr-4 focus:border-secondary outline-none placeholder-gray-600 transition-all text-base"
                    >
                </div>

                <div class="w-full md:w-48">
                    <select v-model="filters.sort" @change="fetchData()" class="w-full bg-input text-gray-300 border border-input-border rounded-lg py-3 px-4 focus:border-secondary outline-none transition-all text-base appearance-none cursor-pointer">
                        <option value="newest">📅 En Yeni</option>
                        <option value="bestsellers">🔥 En Çok Satanlar</option> 
                        <option value="oldest">📅 En Eski</option>
                        <option value="price_asc">💰 Fiyat Artan</option>
                        <option value="price_desc">💰 Fiyat Azalan</option>
                        <option value="stock_desc">📦 Stok Azalan</option>
                        <option value="stock_asc">📦 Stok Artan</option>
                    </select>
                </div>

                <div class="w-full md:w-48 relative">
                    <select v-model="filters.category" @change="fetchData()" class="w-full bg-input text-gray-300 border border-input-border rounded-lg py-3 px-4 focus:border-secondary outline-none transition-all text-base appearance-none cursor-pointer">
                        <option :value="null">📂 Tüm Kategoriler</option>
                        <option v-for="cat in categories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
                    </select>
                    <div class="absolute right-4 top-1/2 -translate-y-1/2 pointer-events-none text-gray-500">▼</div>
                </div>

            </div>

            <ProductTable 
                :products="products"
                :is-loading="isLoading"
                @edit="openEditModal"
                @delete="handleDeleteWrapper"
                @preview-image="openLightbox"
            />

            <div v-if="totalPages > 1" class="p-4 border-t border-card-border flex items-center justify-between bg-input">
                <span class="text-sm text-gray-500">
                    {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} / {{ totalCount }}
                </span>
                <div class="flex items-center space-x-2">
                    <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="px-3 py-1 rounded bg-card-border text-gray-300 hover:bg-gray-700 disabled:opacity-50 border border-input-border">&lt;</button>
                    <div class="flex space-x-1">
                        <button v-for="p in totalPages" :key="p" @click="changePage(p)" v-show="Math.abs(p - currentPage) < 3 || p === 1 || p === totalPages" :class="['px-3 py-1 rounded border text-sm font-medium', currentPage === p ? 'bg-primary text-white border-primary' : 'bg-card-border text-gray-300 border-input-border hover:bg-gray-700']">{{ p }}</button>
                    </div>
                    <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="px-3 py-1 rounded bg-card-border text-gray-300 hover:bg-gray-700 disabled:opacity-50 border border-input-border">&gt;</button>
                </div>
            </div>
        </div>

        <ProductFormModal 
            v-if="showModal"
            :is-open="showModal" 
            :edit-mode="isEditing" 
            :product-data="selectedProduct"
            @close="showModal = false" 
            @save-success="handleSave"
        />

        <ImageLightbox 
            :show="showLightbox"
            :image-src="lightboxImage"
            @close="showLightbox = false"
        />

    </div>
</template>