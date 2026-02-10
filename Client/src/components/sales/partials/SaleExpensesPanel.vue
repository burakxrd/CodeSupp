<script setup lang="ts">
import { computed, ref, onMounted } from 'vue';
import { useSettingsStore } from '../../../stores/settings';
import SmartExpenseInput from './SmartExpenseInput.vue';

// --- TYPES ---
interface SaleModel {
    manualDiscount: number;
    platformCommission: number;
    taxAmount: number;
    shippingCost: number;
    externalReference: string;
    [key: string]: any;
}

interface ExpenseProfile {
    id: string;
    name: string;
    comm: number;
    tax: number;
    ship: number;
}

interface Tab {
    id: string;
    label: string;
    color: string;
}

interface Props {
    modelValue: SaleModel;
    productsTotal?: number;
}

// --- PROPS ---
const props = withDefaults(defineProps<Props>(), {
    productsTotal: 0
});

// --- STORE ---
const settingsStore = useSettingsStore();

// --- STATE ---
const activeTab = ref<string>('kesintiler'); 
const selectedProfile = ref<string>('');

// Tab Tanımları
const tabs: Tab[] = [
    { id: 'kesintiler', label: 'Kesintiler', color: 'blue' },
    { id: 'kargo', label: 'Kargo', color: 'blue' },
    { id: 'sonuc', label: 'Sonuç', color: 'teal' }
];

// Profil Sistemi
const expenseProfiles: ExpenseProfile[] = [
    { id: 'elden', name: 'Elden / Nakit', comm: 0, tax: 0, ship: 0 },
    { id: 'shopier', name: 'Shopier (%5 + Kargo)', comm: 5, tax: 20, ship: 45 },
    { id: 'trendyol', name: 'Trendyol (%21)', comm: 21, tax: 20, ship: 40 },
    { id: 'hepsiburada', name: 'Hepsiburada (%20)', comm: 20, tax: 20, ship: 40 },
];

const applyProfile = () => {
    const p = expenseProfiles.find(x => x.id === selectedProfile.value);
    if (!p) return;

    // Sadece kargo maliyetini atıyoruz, diğerlerini SmartInput bileşenleri
    // kullanıcı manuel girmek isteyebilir diye ellemiyoruz veya
    // buraya daha gelişmiş bir logic eklenebilir.
    props.modelValue.shippingCost = p.ship;
};

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val);
};

// Hesaplamalar
const customerTotal = computed(() => Math.max(0, props.productsTotal - (props.modelValue.manualDiscount || 0)));
const totalExpenses = computed(() => (props.modelValue.shippingCost || 0) + (props.modelValue.platformCommission || 0) + (props.modelValue.taxAmount || 0));
const netProfit = computed(() => customerTotal.value - totalExpenses.value);
const profitClass = computed(() => {
    if (netProfit.value > 0) return 'text-green-400';
    if (netProfit.value < 0) return 'text-red-400';
    return 'text-gray-400';
});

onMounted(() => {
    // Vergi alanı undefined gelirse 0'a eşitle
    if (props.modelValue.taxAmount === undefined) {
        props.modelValue.taxAmount = 0;
    }
});
</script>

<template>
    <div class="bg-[#151521]/50 border border-gray-700 rounded-xl h-full flex flex-col overflow-hidden relative">
        
        <div class="bg-[#151521] border-b border-gray-700 p-2">
            <div class="flex justify-between items-center mb-3 px-2">
                <h4 class="text-white font-bold text-sm flex items-center gap-2"><span>📉</span> Maliyet Yönetimi</h4>
                <select v-model="selectedProfile" @change="applyProfile" class="bg-gray-800 text-[10px] text-gray-300 border border-gray-600 rounded px-2 py-1 outline-none hover:border-blue-500 cursor-pointer">
                    <option value="" disabled selected>Profil Yükle...</option>
                    <option v-for="p in expenseProfiles" :key="p.id" :value="p.id">{{ p.name }}</option>
                </select>
            </div>
            
            <div class="flex bg-gray-800/50 p-1 rounded-lg gap-1">
                <button 
                    v-for="tab in tabs" :key="tab.id" @click="activeTab = tab.id"
                    :class="['flex-1 py-1.5 text-xs font-bold rounded-md transition-all duration-300', activeTab === tab.id ? `bg-gradient-to-r from-${tab.color}-600 to-${tab.color}-500 text-white shadow-lg` : 'text-gray-400 hover:text-white hover:bg-white/10']"
                >{{ tab.label }}</button>
            </div>
        </div>

        <div class="flex-1 overflow-y-auto p-5 custom-scrollbar">
            
            <div v-if="activeTab === 'kesintiler'" class="space-y-6">
                <SmartExpenseInput 
                    label="Sepet İndirimi" color="orange" 
                    v-model="modelValue.manualDiscount" 
                />

                <SmartExpenseInput 
                    label="Platform Komisyonu" color="purple" 
                    v-model="modelValue.platformCommission"
                    :allow-percentage="true"
                    :base-amount="productsTotal"
                />

                <SmartExpenseInput 
                    v-if="settingsStore.enableVAT"
                    label="Vergi / KDV" color="cyan" 
                    v-model="modelValue.taxAmount"
                    :allow-percentage="true"
                    :base-amount="productsTotal"
                    :default-rate="settingsStore.defaultVAT"
                />
            </div>

            <div v-if="activeTab === 'kargo'" class="space-y-6">
                <SmartExpenseInput 
                    label="Kargo Maliyeti (Gider)" color="red" 
                    v-model="modelValue.shippingCost"
                >
                    <template #footer>
                        <p class="text-[10px] text-gray-500 mt-1">* Kargo firmasına ödeyeceğiniz tutar.</p>
                    </template>
                </SmartExpenseInput>

                <div>
                    <label class="block text-blue-400 text-xs uppercase font-bold mb-2">Sipariş / Fatura No</label>
                    <div class="relative group">
                        <span class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500 font-bold pointer-events-none">#</span>
                        <input v-model="modelValue.externalReference" type="text" placeholder="Örn: PAZARYERI-123" class="w-full bg-gray-800 border border-gray-600 rounded-lg pl-7 pr-3 py-3 text-white text-sm focus:border-blue-500 outline-none transition-colors">
                    </div>
                </div>
            </div>

            <div v-if="activeTab === 'sonuc'" class="space-y-4 h-full flex flex-col">
                <div class="bg-[#1E1E2D] rounded-lg p-4 border border-dashed border-gray-600 space-y-3 shadow-inner">
                    <div class="flex justify-between text-sm"><span class="text-gray-400">Ürünler:</span><span class="text-white">{{ formatCurrency(productsTotal) }}</span></div>
                    <div class="flex justify-between text-sm"><span class="text-orange-400">(-) İndirim:</span><span class="text-orange-400">{{ formatCurrency(modelValue.manualDiscount) }}</span></div>
                    <div class="border-t border-gray-700 pt-2 flex justify-between"><span class="text-white font-bold">Ciro:</span><span class="text-white font-bold">{{ formatCurrency(customerTotal) }}</span></div>
                    
                    <div class="space-y-1 pt-2 border-t border-gray-700/50 text-xs">
                        <div class="flex justify-between text-red-300/70"><span>(-) Kargo:</span><span>{{ formatCurrency(modelValue.shippingCost) }}</span></div>
                        <div class="flex justify-between text-purple-300/70"><span>(-) Komisyon:</span><span>{{ formatCurrency(modelValue.platformCommission) }}</span></div>
                        <div class="flex justify-between text-cyan-300/70"><span>(-) Vergi:</span><span>{{ formatCurrency(modelValue.taxAmount) }}</span></div>
                    </div>
                </div>
                <div class="mt-auto bg-gray-800/50 p-4 rounded-xl border border-gray-700 text-center shadow-lg">
                    <span class="block text-gray-400 text-xs uppercase font-bold mb-1">Tahmini Net Kazanç</span>
                    <span :class="['font-mono font-black text-3xl', profitClass]">{{ formatCurrency(netProfit) }}</span>
                </div>
            </div>
        </div>

        <div v-if="activeTab !== 'sonuc'" class="p-3 border-t border-gray-700 bg-[#151521] flex justify-between items-center">
             <span class="text-xs text-gray-500">Net Kazanç:</span>
             <span :class="['font-bold text-sm font-mono', profitClass]">{{ formatCurrency(netProfit) }}</span>
        </div>
    </div>
</template>