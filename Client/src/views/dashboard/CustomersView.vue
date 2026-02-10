<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import api from '../../services/api';
import { useDataTable } from '../../composables/useDataTable';
import { useToast } from "vue-toastification";
import type { Customer } from '../../schemas/customerSchema';

import CustomerStatusFilter from '../../components/customers/CustomerStatusFilter.vue';
import CustomerListTable from '../../components/customers/CustomerListTable.vue'; 
import CustomerFormModal from '../../components/customers/CustomerFormModal.vue';
import CustomerDetailModal from '../../components/customers/CustomerDetailModal.vue';

const toast = useToast();

// --- 1. FİLTRELEME ---
const currentStatus = ref<string | undefined>(undefined); 
watch(currentStatus, () => fetchData(1));

// --- 2. DATATABLE ---
interface FetchParams {
    page: number;
    pageSize: number;
    search: string;
}

const fetchCustomersAdapter = async (p: FetchParams) => {
    const res = await api.getCustomers(p.page, p.pageSize, p.search, currentStatus.value || null);
    return {
        items: res.items,
        totalCount: res.total,
        totalPages: Math.ceil(res.total / p.pageSize),
        pageNumber: res.page,
        pageSize: res.pageSize
    };
};

// apiDelete Wrapper: ID'yi number'a çevirir
const deleteCustomerWrapper = async (id: string | number) => {
    return await api.deleteCustomer(Number(id));
};

const {
    items: customers,
    isLoading,
    searchQuery,
    currentPage,
    totalPages,
    totalCount,
    pageSize,
    fetchData,
    changePage,
    handleDelete
} = useDataTable<Customer>({
    apiFetch: fetchCustomersAdapter,
    apiDelete: deleteCustomerWrapper,
    pageSizeInit: 20
});

// --- 3. MODALLAR ---
const showFormModal = ref<boolean>(false);
const selectedCustomerForForm = ref<Customer | undefined>(undefined);
const showDetailModal = ref<boolean>(false);
const selectedCustomerForDetail = ref<Customer | undefined>(undefined);

const openAddModal = () => { 
    selectedCustomerForForm.value = undefined; 
    showFormModal.value = true; 
};
const openEditModal = (c: Customer) => { 
    selectedCustomerForForm.value = c; 
    showFormModal.value = true; 
};
const openDetailModal = (c: Customer) => { 
    selectedCustomerForDetail.value = c; 
    showDetailModal.value = true; 
};
const handleSaveSuccess = () => { fetchData(currentPage.value); };

const onDelete = async (id: number) => {
    const success = await handleDelete(id, 'Bu müşteriyi silmek istediğine emin misin?');
    if (success) toast.success("Müşteri silindi.");
};

onMounted(() => fetchData());
</script>

<template>
    <div class="min-h-screen text-gray-300 font-sans flex flex-col h-screen overflow-hidden bg-[#151521]">
        
        <div class="flex-shrink-0 px-8 py-6 flex justify-between items-end">
            <div>
                <h1 class="text-3xl font-bold text-white tracking-tight">Müşteriler</h1>
                <p class="text-gray-500 text-sm mt-1 font-medium">Toplam <span class="text-gray-300">{{ totalCount }}</span> müşteri kayıtlı.</p>
            </div>
            
            <button 
                @click="openAddModal" 
                class="bg-[#3699FF] hover:bg-[#2F88E6] text-white px-5 py-2.5 rounded-xl font-semibold text-sm shadow-lg shadow-blue-500/20 flex items-center gap-2 transition-all transform active:scale-95"
            >
                <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fill-rule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clip-rule="evenodd" />
                </svg>
                Yeni Müşteri
            </button>
        </div>

        <div class="flex-1 mx-8 mb-8 bg-[#1E1E2D] rounded-2xl shadow-xl border border-gray-800/60 flex flex-col overflow-hidden">
            
            <div class="p-4 border-b border-gray-800/80 flex flex-col md:flex-row gap-4 items-center justify-between bg-[#1E1E2D]">
                
                <div class="w-full md:w-auto overflow-x-auto pb-1 md:pb-0 scrollbar-hide">
                    <CustomerStatusFilter :modelValue="currentStatus" @update:modelValue="val => currentStatus = val || undefined" />
                </div>

                <div class="relative w-full md:w-72 group">
                    <span class="absolute inset-y-0 left-0 flex items-center pl-3 text-gray-500 group-focus-within:text-[#3699FF] transition-colors">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                    </span>
                    <input 
                        v-model="searchQuery" 
                        type="text" 
                        placeholder="Ad, E-posta veya Instagram..." 
                        class="w-full bg-[#151521] text-gray-300 border border-gray-700 rounded-xl py-2.5 pl-10 pr-4 focus:border-[#3699FF] focus:ring-1 focus:ring-[#3699FF] outline-none text-sm transition-all placeholder-gray-600 font-medium"
                    >
                </div>
            </div>

            <CustomerListTable 
                :customers="customers" 
                :is-loading="isLoading"
                @row-click="openDetailModal"
                @edit="openEditModal"
                @delete="onDelete"
            />

            <div v-if="customers.length > 0" class="p-4 border-t border-gray-800/80 flex items-center justify-between bg-[#1E1E2D]">
                <span class="text-xs text-gray-500 font-medium">
                    {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, totalCount) }} arası gösteriliyor
                </span>
                <div class="flex items-center gap-2">
                    <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1" class="w-9 h-9 rounded-lg flex items-center justify-center bg-[#2B2B40] text-gray-400 hover:text-white hover:bg-[#363650] disabled:opacity-30 transition-colors border border-gray-700/50">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clip-rule="evenodd" /></svg>
                    </button>
                    <div class="px-3 py-1.5 rounded-lg bg-[#151521] text-xs font-mono text-gray-400 border border-gray-800">
                        {{ currentPage }} / {{ totalPages }}
                    </div>
                    <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages" class="w-9 h-9 rounded-lg flex items-center justify-center bg-[#2B2B40] text-gray-400 hover:text-white hover:bg-[#363650] disabled:opacity-30 transition-colors border border-gray-700/50">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" /></svg>
                    </button>
                </div>
            </div>
            
        </div>

        <CustomerFormModal 
            :is-open="showFormModal" 
            :customer-to-edit="selectedCustomerForForm"
            @close="showFormModal = false"
            @save-success="handleSaveSuccess"
        />

        <CustomerDetailModal
            :is-open="showDetailModal"
            :customer="selectedCustomerForDetail"
            @close="showDetailModal = false"
        />

    </div>
</template>

<style scoped>
.scrollbar-hide::-webkit-scrollbar { display: none; }
.scrollbar-hide { -ms-overflow-style: none; scrollbar-width: none; }
</style>