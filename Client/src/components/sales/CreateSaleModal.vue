<script setup lang="ts">
import { ref, computed } from 'vue';
// [DÜZELTME 1] Servis isimlendirmesi ve kullanımı eşitlendi
import saleService from '../../services/saleService';

// --- IMPORT PARTIALS ---
import SaleCustomerSelect from '../../components/sales/partials/SaleCustomerSelect.vue';
import SaleItemsTable from '../../components/sales/partials/SaleItemsTable.vue';
import SaleExpensesPanel from '../../components/sales/partials/SaleExpensesPanel.vue';

// --- TYPES ---
interface SaleItemState {
    productId: number | string; 
    quantity: number; 
    unitPrice: number;
    searchQuery?: string;
    showDropdown?: boolean;
}

interface SaleFormState {
    customerId: number | string;
    saleDate: string;
    shippingCost: number;
    platformCommission: number;
    manualDiscount: number;
    taxAmount: number;
    externalReference: string;
    items: SaleItemState[];
}

interface Props {
    isOpen: boolean;
    customers?: any[];
    products?: any[]; 
}

const props = withDefaults(defineProps<Props>(), {
    customers: () => [],
    products: () => []
});

const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'sale-created'): void;
}>();

const INITIAL_FORM_STATE: SaleFormState = {
    customerId: '',
    saleDate: new Date().toISOString().substring(0, 10),
    shippingCost: 0,
    platformCommission: 0,
    manualDiscount: 0,
    taxAmount: 0,
    externalReference: '',
    items: [{ productId: '', quantity: 1, unitPrice: 0, searchQuery: '', showDropdown: false }]
};

const newSale = ref<SaleFormState>(JSON.parse(JSON.stringify(INITIAL_FORM_STATE)));
const customerSelectRef = ref<any>(null);

const productsTotal = computed(() => {
    return newSale.value.items.reduce((sum, item) => sum + (Number(item.quantity || 0) * Number(item.unitPrice || 0)), 0);
});

const isFormValid = computed(() => {
    if (!newSale.value.customerId) return false;
    
    const validItems = newSale.value.items.filter(i => i.productId && i.quantity > 0);
    if (validItems.length === 0) return false;
    
    if (customerSelectRef.value?.isAddingCustomer) return false;

    return true;
});

const resetForm = () => {
    newSale.value = JSON.parse(JSON.stringify(INITIAL_FORM_STATE));
};

const handleCustomerAdded = (newCustomer: any) => {
    // Yeni müşteri eklendiğinde yapılacak işlemler buraya
};

const saveSale = async () => {
    if (!isFormValid.value) return;

    try {
        const validItems = newSale.value.items.filter(i => i.productId && i.quantity > 0);
        
        const payload = {
            customerId: Number(newSale.value.customerId),
            saleDate: newSale.value.saleDate,
            shippingCost: Number(newSale.value.shippingCost) || 0,
            platformCommission: Number(newSale.value.platformCommission) || 0,
            manualDiscount: Number(newSale.value.manualDiscount) || 0,
            taxAmount: Number(newSale.value.taxAmount) || 0,
            externalReference: newSale.value.externalReference,
            items: validItems.map(i => ({
                productId: Number(i.productId),
                quantity: Number(i.quantity),
                unitPrice: Number(i.unitPrice)
            }))
        };

        // [DÜZELTME 3] orderService yerine yukarıdaki saleService kullanıldı
        await saleService.createSale(payload);
        
        alert('Sipariş başarıyla oluşturuldu!'); 
        resetForm();
        emit('sale-created');
        emit('close');
        
    } catch (err: any) {
        console.error("Sipariş Oluşturma Hatası:", err);
        const msg = err.response?.data?.message || err.customMessage || 'Bir hata oluştu.';
        alert("Hata: " + msg);
    }
};
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-[999] flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 overflow-hidden">
        <div class="bg-[#1E1E2D] w-full max-w-7xl h-[90vh] rounded-2xl shadow-2xl border border-gray-700 flex flex-col relative overflow-hidden animate-zoom-in">
            <div class="flex justify-between items-center px-8 py-5 border-b border-gray-700 bg-[#151521]">
                <h3 class="text-2xl font-bold text-white flex items-center gap-3">
                    <span class="bg-blue-600/20 text-blue-500 p-2 rounded-lg">🛒</span>
                    Yeni Sipariş Oluştur
                </h3>
                <button @click="emit('close')" class="text-gray-500 hover:text-white transition-colors bg-gray-800 hover:bg-red-500/20 hover:text-red-500 rounded-lg p-2">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                    </svg>
                </button>
            </div>

            <div class="flex-1 overflow-hidden p-6 grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div class="lg:col-span-2 flex flex-col gap-6 h-full overflow-hidden">
                    <div class="bg-[#151521]/50 p-5 rounded-xl border border-gray-700 grid grid-cols-1 md:grid-cols-2 gap-6">
                        <SaleCustomerSelect 
                            ref="customerSelectRef"
                            v-model="newSale.customerId" 
                            :customers="customers"
                            @customer-added="handleCustomerAdded"
                        />
                        <div>
                            <label class="block text-gray-400 text-sm mb-2 font-medium">Satış Tarihi</label>
                            <input 
                                v-model="newSale.saleDate" 
                                type="date" 
                                class="w-full bg-[#151521] border border-gray-700 rounded-xl px-4 py-3 text-white outline-none focus:border-[#3699FF] transition-all text-base h-12 [color-scheme:dark]" 
                                required
                            >
                        </div>
                    </div>

                    <div class="flex-1 bg-[#151521]/50 p-5 rounded-xl border border-gray-700 overflow-hidden flex flex-col">
                        <h4 class="text-white font-bold text-lg mb-4 flex items-center gap-2">
                            <span>📦</span> Ürün Listesi
                        </h4>
                        <SaleItemsTable 
                            v-model="newSale.items" 
                            :products="products"
                            class="flex-1 min-h-0"
                        />
                    </div>
                </div>

                <div class="lg:col-span-1 h-full overflow-hidden">
                    <SaleExpensesPanel 
                        v-model="newSale" 
                        :productsTotal="productsTotal"
                    />
                </div>
            </div>

            <div class="px-8 py-5 border-t border-gray-700 bg-[#151521] flex justify-end gap-4">
                <button type="button" @click="emit('close')" class="px-6 py-3 rounded-xl font-bold text-gray-400 hover:text-white hover:bg-gray-700 transition-colors">
                    İptal
                </button>
                <button 
                    type="button" 
                    @click="saveSale" 
                    :disabled="!isFormValid"
                    class="bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white px-10 py-3 rounded-xl font-bold shadow-lg shadow-indigo-500/30 transition-all transform active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none flex items-center gap-2"
                >
                    <span>💾</span> Siparişi Kaydet
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.animate-zoom-in {
    animation: zoomIn 0.2s ease-out;
}
@keyframes zoomIn {
    from { opacity: 0; transform: scale(0.95); }
    to { opacity: 1; transform: scale(1); }
}
</style>