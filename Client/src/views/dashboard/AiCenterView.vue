<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../services/api';  
import { useRouter } from 'vue-router';

const router = useRouter();

// --- TYPES ---
interface Product {
    value: string | number;
    text: string;
}

interface PurchaseItem {
    productId: string | number;
    originalName: string;
    quantityInUnits: number;
    productPricePerUnit: number;
    totalKg: number;
    shippingCostPerKg: number;
}

interface OrderProduct {
    productId: string | number;
    originalName: string;
    quantity: number;
    unitPrice: number;
}

interface OrderData {
    CustomerName?: string;
    Phone?: string;
    Address?: string;
    products: OrderProduct[];
    [key: string]: any;
}

// --- STATE ---
const activeTab = ref<'invoice' | 'order'>('invoice'); // 'invoice' (Alım) | 'order' (Satış/DM)
const isAnalyzing = ref<boolean>(false);
const products = ref<Product[]>([]); // Dropdown için ürün listesi

// Veri Alanları
const detectedPurchaseItems = ref<PurchaseItem[]>([]); // Alım (Fatura) Sonuçları
const detectedOrder = ref<OrderData | null>(null); // Satış (DM) Sonucu

// --- API ---
const loadProducts = async () => {
    try {
        const data = await api.getPurchaseCreateData();
        // Backend yapısına göre products items içinde veya direkt gelebilir
        products.value = (data.products?.items || data.products || []) as Product[];
    } catch (e) { 
        console.error(e); 
    }
};

// --- YARDIMCI: Eşleştirme Algoritması ---
const normalizeText = (text: string | null | undefined): string => {
    if (!text) return "";
    return text.toString().toLocaleLowerCase('tr-TR')
        .replace(/ğ/g, 'g').replace(/ü/g, 'u').replace(/ş/g, 's')
        .replace(/ı/g, 'i').replace(/ö/g, 'o').replace(/ç/g, 'c')
        .replace(/[^a-z0-9\s]/g, '').trim();
};

const findBestMatch = (aiProductName: string): string | number => {
    if (!aiProductName) return '';
    const searchTerms = normalizeText(aiProductName).split(/\s+/);
    let bestMatchId: string | number = '';
    let highestScore = 0;

    products.value.forEach(p => {
        const productWords = normalizeText(p.text).split(/\s+/);
        let matchCount = 0;
        searchTerms.forEach(term => {
            if (productWords.some(pw => pw.includes(term) || term.includes(pw))) matchCount++;
        });
        const totalUniqueWords = new Set([...searchTerms, ...productWords]).size;
        const score = matchCount / totalUniqueWords;

        if (score > 0.3 && score > highestScore) {
            highestScore = score;
            bestMatchId = p.value;
        }
    });
    return highestScore > 0.3 ? bestMatchId : ''; 
};

// --- İŞLEMLER ---

const handleAnalyze = async (event: Event) => {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (!file) return;

    isAnalyzing.value = true;
    detectedPurchaseItems.value = [];
    detectedOrder.value = null;

    const formData = new FormData();
    formData.append('file', file);
    formData.append('docType', activeTab.value);

    try {
        const aiResponse = await api.analyzeDocument(formData);
        let aiData = typeof aiResponse === 'string' ? JSON.parse(aiResponse) : aiResponse;

        if (activeTab.value === 'invoice') {
            // --- FATURA/ALIM MODU ---
            if (!Array.isArray(aiData)) aiData = [aiData];
            
            detectedPurchaseItems.value = aiData.map((item: any) => ({
                productId: findBestMatch(item.productName),
                originalName: item.productName,
                quantityInUnits: item.quantity || 1,
                productPricePerUnit: item.unitPrice || 0,
                totalKg: item.totalKg || 0,
                shippingCostPerKg: item.shippingCostPerKg || 0
            }));
        } else {
            // --- DM/SATIŞ MODU ---
            const orderData = Array.isArray(aiData) ? aiData[0] : aiData;
            
            // Ürünleri eşleştir
            if (orderData.products && Array.isArray(orderData.products)) {
                orderData.products = orderData.products.map((p: any) => ({
                    ...p,
                    productId: findBestMatch(p.productName), 
                    originalName: p.productName
                }));
            }
            detectedOrder.value = orderData as OrderData;
        }

    } catch (err: any) {
        console.error("Analiz Hatası:", err);
        alert("Analiz başarısız: " + (err.response?.data || err.message));
    } finally {
        isAnalyzing.value = false;
        target.value = ''; 
    }
};


const savePurchase = async () => {
    const invalidItems = detectedPurchaseItems.value.filter(x => !x.productId);
    if (invalidItems.length > 0) {
        alert("Lütfen tüm satırlar için ürün eşleşmesi yapın.");
        return;
    }
    try {
        await api.post('/product-purchase/bulk', detectedPurchaseItems.value);
        alert("Stok girişi başarılı! 📥");
        router.push('/dashboard/products');
    } catch (err: any) {
        alert("Hata: " + err.message);
    }
};

// B) Sipariş Kaydet (Satış)
const saveOrder = async () => {
    if (!detectedOrder.value) return;
    
    // Ürün eşleşmesi kontrolü
    const invalidProducts = detectedOrder.value.products.filter(p => !p.productId);
    if (invalidProducts.length > 0) {
        alert("Lütfen siparişteki tüm ürünleri sistemdeki ürünlerle eşleştirin.");
        return;
    }

    try {
        // Backend 'List<BulkSaleItemViewModel>' bekliyor, biz tek bir sipariş yolluyoruz, o yüzden dizi içine alıyoruz.
        const payload = [detectedOrder.value]; 
        await api.createBulkSale(payload);
        
        alert("Sipariş başarıyla oluşturuldu! 💰");
        router.push('/dashboard/sales');
    } catch (err: any) {
        alert("Hata: " + (err.response?.data?.message || err.message));
    }
};

onMounted(() => { loadProducts(); });
</script>

<template>
    <div class="min-h-screen text-gray-300">
        
        <div class="mb-8">
            <h1 class="text-3xl font-bold text-white tracking-tight flex items-center gap-3">
                <span class="text-transparent bg-clip-text bg-gradient-to-r from-purple-400 to-pink-600">AI Otomasyon Merkezi</span>
                <span class="text-xs bg-purple-500/20 text-purple-300 px-2 py-1 rounded border border-purple-500/30">BETA</span>
            </h1>
            <p class="text-gray-400 mt-2">Belge ve sohbet görüntülerini işleyerek zaman kazanın.</p>
        </div>

        <div class="flex space-x-1 bg-gray-800/50 p-1 rounded-xl mb-8 w-fit border border-gray-700">
            <button 
                @click="activeTab = 'invoice'; detectedOrder = null; detectedPurchaseItems = []"
                :class="['px-6 py-2 rounded-lg text-sm font-medium transition-all', activeTab === 'invoice' ? 'bg-indigo-600 text-white shadow-lg' : 'text-gray-400 hover:text-white hover:bg-gray-700']"
            >
                📥 Fatura (Stok Girişi)
            </button>
            <button 
                @click="activeTab = 'order'; detectedOrder = null; detectedPurchaseItems = []"
                :class="['px-6 py-2 rounded-lg text-sm font-medium transition-all', activeTab === 'order' ? 'bg-pink-600 text-white shadow-lg' : 'text-gray-400 hover:text-white hover:bg-gray-700']"
            >
                💬 DM Sipariş (Satış)
            </button>
        </div>

        <div v-if="!detectedPurchaseItems.length && !detectedOrder" class="max-w-2xl mx-auto">
            <div class="bg-[#1E1E2D] border-2 border-dashed border-gray-700 hover:border-indigo-500/50 rounded-2xl p-16 text-center transition-all group relative overflow-hidden">
                <div v-if="!isAnalyzing">
                    <div class="w-20 h-20 bg-gray-800 rounded-full flex items-center justify-center mx-auto mb-6 group-hover:scale-110 transition-transform">
                        <span class="text-4xl">{{ activeTab === 'invoice' ? '📄' : '📱' }}</span>
                    </div>
                    <h3 class="text-white font-bold text-xl mb-3">
                        {{ activeTab === 'invoice' ? 'Toptancı Faturası Yükle' : 'DM / WhatsApp Ekran Görüntüsü Yükle' }}
                    </h3>
                    <p class="text-gray-500 mb-8">
                        {{ activeTab === 'invoice' ? 'Toplu ürün alımlarını stoğa işler.' : 'Sohbetten müşteri ve sipariş bilgilerini çıkarır.' }}
                    </p>
                    <label :class="['cursor-pointer text-white px-10 py-4 rounded-xl font-bold shadow-lg inline-block transition-all transform hover:-translate-y-1', activeTab === 'invoice' ? 'bg-indigo-600 hover:bg-indigo-500 shadow-indigo-500/25' : 'bg-pink-600 hover:bg-pink-500 shadow-pink-500/25']">
                        Görsel Seç ve Analiz Et
                        <input type="file" class="hidden" accept="image/*,.pdf" @change="handleAnalyze">
                    </label>
                </div>
                <div v-else class="py-10">
                    <div class="relative w-24 h-24 mx-auto mb-6">
                        <div class="absolute inset-0 border-4 border-gray-700 rounded-full"></div>
                        <div :class="['absolute inset-0 border-4 rounded-full border-t-transparent animate-spin', activeTab === 'invoice' ? 'border-indigo-500' : 'border-pink-500']"></div>
                        <div class="absolute inset-0 flex items-center justify-center text-3xl animate-pulse">🧠</div>
                    </div>
                    <h3 class="text-white font-bold text-xl animate-pulse">Yapay Zeka Okuyor...</h3>
                </div>
            </div>
        </div>

        <div v-if="detectedPurchaseItems.length > 0 && activeTab === 'invoice'" class="bg-[#1E1E2D] border border-gray-700 rounded-2xl overflow-hidden shadow-2xl">
            <div class="p-6 border-b border-gray-700 flex justify-between items-center bg-gray-800/50">
                <h3 class="text-lg font-bold text-white">Fatura Analizi</h3>
                <div class="flex gap-3">
                    <button @click="detectedPurchaseItems = []" class="text-gray-400 hover:text-white px-4">İptal</button>
                    <button @click="savePurchase" class="bg-indigo-600 hover:bg-indigo-500 text-white px-6 py-2 rounded-lg font-bold">Kaydet</button>
                </div>
            </div>
            <div class="overflow-x-auto">
                <table class="w-full text-left border-collapse">
                    <thead>
                        <tr class="bg-gray-900/50 text-gray-400 text-xs uppercase">
                            <th class="p-4">AI Tespiti</th>
                            <th class="p-4">Stok Ürünü</th>
                            <th class="p-4 w-24">Adet</th>
                            <th class="p-4 w-32">Fiyat</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-gray-700">
                        <tr v-for="(item, idx) in detectedPurchaseItems" :key="idx">
                            <td class="p-4 text-gray-400 text-sm">{{ item.originalName }}</td>
                            <td class="p-4">
                                <select v-model="item.productId" class="w-full bg-[#151521] border border-gray-600 rounded p-2 text-white text-sm outline-none focus:border-indigo-500">
                                    <option value="" disabled>Seçiniz...</option>
                                    <option v-for="p in products" :key="p.value" :value="p.value">{{ p.text }}</option>
                                </select>
                            </td>
                            <td class="p-4"><input v-model="item.quantityInUnits" type="number" class="w-full bg-transparent border-b border-gray-600 text-center text-white"></td>
                            <td class="p-4"><input v-model="item.productPricePerUnit" type="number" step="0.01" class="w-full bg-transparent border-b border-gray-600 text-right text-white"></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div v-if="detectedOrder && activeTab === 'order'" class="bg-[#1E1E2D] border border-gray-700 rounded-2xl overflow-hidden shadow-2xl p-6">
            <div class="flex justify-between items-start mb-6 border-b border-gray-700 pb-4">
                <div>
                    <h3 class="text-xl font-bold text-white mb-1">Sipariş Taslağı</h3>
                    <p class="text-sm text-gray-400">Yapay zeka sohbetten aşağıdaki bilgileri çıkardı.</p>
                </div>
                <div class="flex gap-3">
                    <button @click="detectedOrder = null" class="text-gray-400 hover:text-white px-4">Vazgeç</button>
                    <button @click="saveOrder" class="bg-pink-600 hover:bg-pink-500 text-white px-6 py-2 rounded-lg font-bold shadow-lg shadow-pink-500/20">
                        Siparişi Oluştur
                    </button>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
                <div class="lg:col-span-1 space-y-4">
                    <h4 class="text-pink-400 font-bold text-sm uppercase tracking-wide mb-4">Müşteri Bilgileri</h4>
                    <div>
                        <label class="block text-gray-500 text-xs mb-1">Ad Soyad / Instagram</label>
                        <input v-if="detectedOrder" v-model="detectedOrder.CustomerName" type="text" class="w-full bg-[#151521] border border-gray-700 rounded-lg p-3 text-white focus:border-pink-500 outline-none">
                    </div>
                    <div>
                        <label class="block text-gray-500 text-xs mb-1">Telefon</label>
                        <input v-if="detectedOrder" v-model="detectedOrder.Phone" type="text" class="w-full bg-[#151521] border border-gray-700 rounded-lg p-3 text-white focus:border-pink-500 outline-none">
                    </div>
                    <div>
                        <label class="block text-gray-500 text-xs mb-1">Adres / Notlar</label>
                        <textarea v-if="detectedOrder" v-model="detectedOrder.Address" rows="4" class="w-full bg-[#151521] border border-gray-700 rounded-lg p-3 text-white focus:border-pink-500 outline-none"></textarea>
                    </div>
                </div>

                <div class="lg:col-span-2">
                    <h4 class="text-pink-400 font-bold text-sm uppercase tracking-wide mb-4">Sipariş İçeriği</h4>
                    <div class="bg-[#151521] rounded-xl border border-gray-700 overflow-hidden">
                        <table class="w-full text-left">
                            <thead class="bg-gray-800 text-gray-400 text-xs uppercase">
                                <tr>
                                    <th class="p-3">Tespit Edilen</th>
                                    <th class="p-3">Eşleşen Ürün</th>
                                    <th class="p-3 w-20 text-center">Adet</th>
                                    <th class="p-3 w-24 text-right">Birim (TL)</th>
                                    <th class="p-3 w-10"></th>
                                </tr>
                            </thead>
                            <tbody v-if="detectedOrder" class="divide-y divide-gray-700">
                                <tr v-for="(prod, i) in detectedOrder.products" :key="i">
                                    <td class="p-3 text-sm text-gray-400">{{ prod.originalName }}</td>
                                    <td class="p-3">
                                        <select v-model="prod.productId" class="w-full bg-gray-800 border border-gray-600 rounded p-2 text-white text-sm outline-none focus:border-pink-500">
                                            <option value="" disabled>Seçiniz...</option>
                                            <option v-for="p in products" :key="p.value" :value="p.value">{{ p.text }}</option>
                                        </select>
                                    </td>
                                    <td class="p-3"><input v-model="prod.quantity" type="number" class="w-full bg-transparent border-b border-gray-600 text-center text-white focus:border-pink-500 outline-none"></td>
                                    <td class="p-3"><input v-model="prod.unitPrice" type="number" class="w-full bg-transparent border-b border-gray-600 text-right text-white focus:border-pink-500 outline-none"></td>
                                    <td class="p-3 text-center">
                                        <button @click="detectedOrder!.products.splice(i, 1)" class="text-gray-600 hover:text-red-400">×</button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                        <div v-if="detectedOrder" class="p-3 bg-gray-800/50 text-right">
                            <span class="text-gray-400 text-sm mr-2">Toplam Tutar:</span>
                            <span class="text-white font-bold text-lg">
                                {{ new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(detectedOrder.products.reduce((sum, p) => sum + (p.quantity * p.unitPrice), 0)) }}
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</template>