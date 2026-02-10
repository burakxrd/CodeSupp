<script setup lang="ts">
import { computed } from 'vue';

// --- TYPES ---

interface OrderProduct {
    name: string;
    quantity: number;
    price?: number;
}

// Backend'den gelen sipariş objesi farklı formatlarda olabiliyor
// Bu yüzden kapsayıcı bir interface tanımlıyoruz.
interface OrderDetail {
    id: number;
    orderCode?: string;
    
    // Tarih alanları
    date?: string;
    saleDate?: string;
    
    // Müşteri & Durum
    customerName?: string;
    status?: string;
    paymentStatus?: string;
    
    // Finansal Veriler
    amount?: number;
    totalAmount?: number;
    manualDiscount?: number;
    shippingCost?: number;
    platformCommission?: number;
    taxAmount?: number;
    
    // Ürünler (İki farklı formatta gelebilir)
    products?: OrderProduct[];
    productNames?: string[];
}

// --- PROPS ---
interface Props {
    isOpen: boolean;
    order?: OrderDetail | null;
}

const props = withDefaults(defineProps<Props>(), {
    isOpen: false,
    order: null
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'close'): void;
    (e: 'navigate'): void;
}>();

// --- HELPERS ---
const formatCurrency = (value?: number | null): string => {
    if (value === undefined || value === null) return '0,00 ₺';
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value);
};

const formatDate = (dateString?: string): string => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('tr-TR', { day: 'numeric', month: 'long', year: 'numeric' });
};

// --- COMPUTED ---

// Prop null gelse bile boş obje döndürerek template'de hata almayı engeller
const safeOrder = computed<OrderDetail>(() => props.order || { id: 0 });

// Gider Toplamı
const totalExpenses = computed(() => {
    const o = safeOrder.value;
    return (o.shippingCost || 0) + (o.platformCommission || 0) + (o.taxAmount || 0);
});

// Net Kazanç Hesabı
const netProfit = computed(() => {
    const o = safeOrder.value;
    // Farklı isimlendirmeleri handle ediyoruz
    const revenue = o.totalAmount ?? o.amount ?? 0; 
    const discount = o.manualDiscount ?? 0;
    
    // Ciro - İndirim - Giderler
    return revenue - discount - totalExpenses.value; 
});
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-[999] flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 transition-all" @click.self="emit('close')">
         
         <div class="bg-[#1E1E2D] w-full max-w-lg rounded-xl shadow-2xl border border-gray-700 flex flex-col max-h-[90vh] animate-fade-in-up">
             
             <div class="flex justify-between items-center p-5 border-b border-gray-700 bg-[#151521] rounded-t-xl">
                 <div>
                     <h3 class="text-lg font-bold text-white flex items-center gap-2">
                         <span>🧾</span> Sipariş Detayı
                         <span class="text-gray-500 font-mono text-sm">#{{ safeOrder.orderCode || safeOrder.id }}</span>
                     </h3>
                     <span class="text-xs text-gray-500">{{ formatDate(safeOrder.date || safeOrder.saleDate) }}</span>
                 </div>
                 <button @click="emit('close')" class="text-gray-400 hover:text-white p-2 hover:bg-white/10 rounded-lg transition-colors">
                     <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" /></svg>
                 </button>
             </div>

             <div class="p-6 overflow-y-auto custom-scrollbar space-y-6" v-if="props.order">
                 
                 <div class="flex gap-4">
                     <div class="bg-gray-800/50 p-3 rounded-xl border border-gray-700 flex-1">
                        <span class="text-[10px] uppercase font-bold text-gray-500 block mb-1">Müşteri</span>
                        <div class="font-medium text-white">{{ safeOrder.customerName || 'Misafir' }}</div>
                     </div>
                     <div class="bg-gray-800/50 p-3 rounded-xl border border-gray-700 flex-1">
                        <span class="text-[10px] uppercase font-bold text-gray-500 block mb-1">Durum</span>
                        <div class="flex items-center gap-2">
                            <span :class="{'text-sm font-medium': true, 
                                'text-emerald-400': safeOrder.status === 'Tamamlandı',
                                'text-amber-400': safeOrder.status === 'Bekliyor',
                                'text-white': !safeOrder.status
                            }">
                                {{ safeOrder.status || safeOrder.paymentStatus || 'Bilinmiyor' }}
                            </span>
                        </div>
                     </div>
                 </div>

                 <div>
                     <h4 class="text-xs font-bold text-gray-400 uppercase mb-2">Ürünler</h4>
                     <div class="bg-gray-800 rounded-xl border border-gray-700 overflow-hidden">
                         <div v-if="safeOrder.products && safeOrder.products.length > 0">
                             <div v-for="(prod, idx) in safeOrder.products" :key="idx" class="p-3 border-b border-gray-700 last:border-0 flex justify-between items-center text-sm">
                                 <div class="text-white">{{ prod.name }}</div>
                                 <div class="text-gray-400 font-mono">x{{ prod.quantity }}</div>
                             </div>
                         </div>
                         <div v-else-if="safeOrder.productNames && safeOrder.productNames.length > 0">
                             <div v-for="(name, idx) in safeOrder.productNames" :key="idx" class="p-3 border-b border-gray-700 last:border-0 text-white text-sm">
                                 {{ name }}
                             </div>
                         </div>
                         <div v-else class="p-3 text-gray-500 text-sm italic">Ürün bilgisi yok.</div>
                     </div>
                 </div>

                 <div>
                     <h4 class="text-xs font-bold text-gray-400 uppercase mb-2">Finansal Detaylar</h4>
                     <div class="bg-gray-800 rounded-xl border border-gray-700 p-4 space-y-2 text-sm">
                         <div class="flex justify-between">
                             <span class="text-gray-400">Ara Toplam (Ciro)</span>
                             <span class="text-white font-bold">{{ formatCurrency(safeOrder.amount || safeOrder.totalAmount) }}</span>
                         </div>
                         
                         <div v-if="(safeOrder.manualDiscount ?? 0) > 0" class="flex justify-between text-orange-400">
                             <span>(-) İndirim</span>
                             <span>{{ formatCurrency(safeOrder.manualDiscount) }}</span>
                         </div>

                         <div class="h-px bg-gray-700 my-2"></div>

                         <div class="flex justify-between text-gray-400 text-xs">
                             <span>(-) Kargo Maliyeti</span>
                             <span>{{ formatCurrency(safeOrder.shippingCost) }}</span>
                         </div>
                         <div class="flex justify-between text-gray-400 text-xs">
                             <span>(-) Komisyon</span>
                             <span>{{ formatCurrency(safeOrder.platformCommission) }}</span>
                         </div>
                         <div class="flex justify-between text-gray-400 text-xs">
                             <span>(-) Vergi / KDV</span>
                             <span>{{ formatCurrency(safeOrder.taxAmount) }}</span>
                         </div>

                         <div class="h-px bg-gray-700 my-2"></div>

                         <div class="flex justify-between items-center pt-1">
                             <span class="text-emerald-400 font-bold uppercase text-xs">Net Kazanç</span>
                             <span class="text-emerald-400 font-black font-mono text-xl">{{ formatCurrency(netProfit) }}</span>
                         </div>
                     </div>
                 </div>

             </div>

             <div class="p-5 border-t border-gray-700 bg-[#151521] rounded-b-xl">
                 <button @click="emit('navigate')" class="w-full bg-gradient-to-r from-indigo-600 to-blue-600 hover:from-indigo-500 hover:to-blue-500 text-white py-3 rounded-xl font-bold shadow-lg shadow-indigo-500/20 transition-all transform active:scale-95 flex items-center justify-center gap-2">
                    <span>✏️</span> Siparişi Yönet / Düzenle
                 </button>
             </div>
         </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #374151; border-radius: 2px; }

@keyframes fadeInUp {
    from { opacity: 0; transform: translateY(20px) scale(0.95); }
    to { opacity: 1; transform: translateY(0) scale(1); }
}
.animate-fade-in-up { animation: fadeInUp 0.3s cubic-bezier(0.16, 1, 0.3, 1); }
</style>