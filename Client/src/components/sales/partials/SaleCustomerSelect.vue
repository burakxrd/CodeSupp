<script setup lang="ts">
import { ref, watch } from 'vue';
import api from '../../../services/api';

// --- TYPES ---
interface Customer {
    id: number;
    name: string;
    phone?: string;
    email?: string;
    isVip?: boolean;
    isLoyal?: boolean;
    isNew?: boolean;
    isRisky?: boolean;
    isPlus75?: boolean;
    [key: string]: any;
}

interface Badge {
    text: string;
    class: string;
}

interface NewCustomerFormState {
    name: string;
    phone: string;
    address: string;
}

// --- PROPS & EMITS ---
interface Props {
    modelValue: string | number;
    initialCustomerName?: string;
}

const props = withDefaults(defineProps<Props>(), {
    modelValue: '',
    initialCustomerName: ''
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: string | number): void;
    (e: 'customer-added', customer: Customer): void;
}>();

// --- LOCAL STATE ---
const searchQuery = ref<string>('');
const searchResults = ref<Customer[]>([]);
const showDropdown = ref<boolean>(false);
const isLoading = ref<boolean>(false);
const selectedCustomerName = ref<string>(''); 
const isAddingCustomer = ref<boolean>(false);
const isSavingCustomer = ref<boolean>(false);
const newCustomerForm = ref<NewCustomerFormState>({ name: '', phone: '', address: '' });

// --- HELPER: Müşteri Statülerini Hesapla ---
const getCustomerBadges = (c: Customer): Badge[] => {
    const badges: Badge[] = [];
    if (c.isVip) badges.push({ text: 'VIP', class: 'bg-purple-500/10 text-purple-400 border-purple-500/20' });
    if (c.isLoyal) badges.push({ text: 'Sadık', class: 'bg-blue-500/10 text-blue-400 border-blue-500/20' });
    if (c.isNew) badges.push({ text: 'Yeni', class: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20' });
    if (c.isRisky) badges.push({ text: 'Riskli', class: 'bg-orange-500/10 text-orange-400 border-orange-500/20' });
    if (c.isPlus75) badges.push({ text: '+75 Gün', class: 'bg-red-500/10 text-red-400 border-red-500/20' });
    return badges;
};

// --- MANUEL DEBOUNCE ---
const debounce = (fn: Function, delay: number) => {
    let timeoutId: any;
    return (...args: any[]) => {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => fn(...args), delay);
    };
};

// --- ARAMA ---
const performSearch = debounce(async (query: string) => {
    if (!query || query.length < 2) {
        searchResults.value = [];
        return;
    }

    try {
        isLoading.value = true;
        // API çağrısı
        const res: any = await api.getCustomers(1, 10, query);
        
        // API Cevabını Garantiye Al
        if (res?.items) searchResults.value = res.items;      
        else if (Array.isArray(res?.data)) searchResults.value = res.data; 
        else if (Array.isArray(res)) searchResults.value = res; 
        else searchResults.value = [];
        
    } catch (err) {
        console.error(err);
    } finally {
        isLoading.value = false;
    }
}, 300);

const handleInput = (e: Event) => {
    const target = e.target as HTMLInputElement;
    const val = target.value;
    searchQuery.value = val;
    selectedCustomerName.value = ''; 
    emit('update:modelValue', ''); 
    showDropdown.value = true;
    performSearch(val);
};

const selectCustomer = (customer: Customer) => {
    selectedCustomerName.value = customer.name;
    searchQuery.value = customer.name; 
    emit('update:modelValue', customer.id);
    showDropdown.value = false;
};

watch(() => props.modelValue, (newId) => {
    if (newId) {
        if (props.initialCustomerName && !selectedCustomerName.value) {
            selectedCustomerName.value = props.initialCustomerName;
            searchQuery.value = props.initialCustomerName;
        } 
    } else {
        selectedCustomerName.value = '';
        if(!isAddingCustomer.value) searchQuery.value = '';
    }
}, { immediate: true });

const toggleAddCustomer = () => {
    isAddingCustomer.value = !isAddingCustomer.value;
    if (isAddingCustomer.value) {
        emit('update:modelValue', '');
        searchQuery.value = '';
        showDropdown.value = false;
    }
};

const saveNewCustomer = async () => {
    if (!newCustomerForm.value.name) return;
    try {
        isSavingCustomer.value = true;
        const res: any = await api.createCustomer(newCustomerForm.value);
        emit('customer-added', res);
        selectCustomer(res);
        isAddingCustomer.value = false;
        newCustomerForm.value = { name: '', phone: '', address: '' };
    } catch (err: any) {
        alert("Hata: " + (err.response?.data?.message || err.message));
    } finally {
        isSavingCustomer.value = false;
    }
};

const closeDropdownDelay = () => {
    setTimeout(() => { showDropdown.value = false; }, 200);
};

defineExpose({ isAddingCustomer });
</script>

<template>
    <div class="relative">
        <label class="block text-gray-400 text-sm mb-2 font-medium">Müşteri Seç *</label>
        
        <div class="flex gap-2 items-start">
            <div class="relative w-full">
                <div class="relative">
                    <span class="absolute left-4 top-1/2 -translate-y-1/2 text-gray-500">
                        {{ isLoading ? '⏳' : '🔍' }}
                    </span>
                    
                    <input 
                        type="text"
                        :value="searchQuery"
                        @input="handleInput"
                        @focus="showDropdown = true"
                        @blur="closeDropdownDelay"
                        :disabled="isAddingCustomer"
                        placeholder="Müşteri adı yazın..."
                        class="w-full bg-[#151521] border border-gray-700 rounded-xl pl-10 pr-10 py-3 text-white outline-none focus:border-[#3699FF] transition-all text-base h-12 disabled:opacity-30 disabled:cursor-not-allowed placeholder-gray-600"
                        autocomplete="off"
                    >

                    <button 
                        v-if="searchQuery && !isAddingCustomer" 
                        @click="() => { searchQuery=''; emit('update:modelValue', ''); }"
                        class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-red-400 p-1"
                    >
                        ✕
                    </button>
                </div>

                <div v-if="showDropdown && searchResults.length > 0" class="absolute left-0 top-full mt-1 w-full z-[9999] bg-[#1E1E2D] border border-gray-700 rounded-xl shadow-2xl max-h-60 overflow-y-auto custom-scrollbar">
                    <ul>
                        <li 
                            v-for="c in searchResults" 
                            :key="c.id"
                            @click="selectCustomer(c)"
                            class="px-4 py-3 hover:bg-[#2B2B40] cursor-pointer text-gray-300 border-b border-gray-800 last:border-0 flex justify-between items-center group"
                        >
                            <div class="flex flex-col">
                                <span class="font-bold text-white group-hover:text-[#3699FF] transition-colors text-sm">
                                    {{ c.name }}
                                </span>
                                <span class="text-xs text-gray-500 font-mono mt-0.5">
                                    {{ c.phone || 'Telefon yok' }}
                                </span>
                                <span v-if="c.email" class="text-[10px] text-gray-500 mt-0.5 truncate max-w-[200px]">
                                    {{ c.email }}
                                </span>
                            </div>

                            <div class="flex flex-col gap-1 items-end pl-2">
                                <span 
                                    v-for="(badge, index) in getCustomerBadges(c)" 
                                    :key="index"
                                    class="text-[10px] px-1.5 py-0.5 rounded border font-medium whitespace-nowrap"
                                    :class="badge.class"
                                >
                                    {{ badge.text }}
                                </span>
                            </div>
                        </li>
                    </ul>
                </div>

                <div v-if="showDropdown && searchQuery.length > 1 && searchResults.length === 0 && !isLoading" class="absolute left-0 top-full mt-1 w-full z-[9999] bg-[#1E1E2D] border border-gray-700 rounded-xl p-4 text-center text-gray-500 text-sm">
                    Sonuç bulunamadı.
                </div>
            </div>

            <button 
                type="button" 
                @click="toggleAddCustomer" 
                :class="['px-4 h-12 rounded-xl transition-colors font-bold text-xl flex-shrink-0', isAddingCustomer ? 'bg-red-500/20 text-red-400 hover:bg-red-500/30' : 'bg-green-500/20 text-green-400 hover:bg-green-500/30']" 
            >
                {{ isAddingCustomer ? '✕' : '+' }}
            </button>
        </div>

        <div v-if="isAddingCustomer" class="mt-4 p-4 bg-[#151521] rounded-xl border border-dashed border-gray-600 animate-fade-in-down">
            <h4 class="text-white font-bold text-sm mb-3 flex items-center gap-2"><span>👤</span> Yeni Müşteri Bilgileri</h4>
            <div class="space-y-3">
                <input v-model="newCustomerForm.name" placeholder="Ad Soyad / Firma Adı *" class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 py-2 text-white text-sm focus:border-green-500 outline-none">
                <div class="grid grid-cols-2 gap-3">
                    <input v-model="newCustomerForm.phone" placeholder="Telefon" class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 py-2 text-white text-sm focus:border-green-500 outline-none">
                    <input v-model="newCustomerForm.address" placeholder="Adres / Şehir" class="w-full bg-gray-800 border border-gray-600 rounded-lg px-3 py-2 text-white text-sm focus:border-green-500 outline-none">
                </div>
                <div class="flex justify-end pt-1">
                    <button type="button" @click="saveNewCustomer" :disabled="isSavingCustomer" class="bg-green-600 hover:bg-green-500 text-white px-4 py-2 rounded-lg text-xs font-bold transition-colors disabled:opacity-50">
                        {{ isSavingCustomer ? 'Kaydediliyor...' : 'Kaydet ve Seç' }}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #1E1E2D; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #333; border-radius: 3px; }
.animate-fade-in-down { animation: fadeInDown 0.3s ease-out; }
@keyframes fadeInDown { from { opacity: 0; transform: translateY(-10px); } to { opacity: 1; transform: translateY(0); } }
</style>