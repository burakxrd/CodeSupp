<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../../services/api'; 
import { useToast } from 'vue-toastification';

const toast = useToast();

// --- TYPES ---
interface Category {
    id: number;
    name: string;
    type: number; // 0: Standart, 1: Giyim
    productCount?: number;
}

interface CategoryPayload {
    id?: number;
    name: string;
    type: number;
}

interface CategoryTypeOption {
    value: number;
    label: string;
}

// --- STATE ---
const categories = ref<Category[]>([]);
const formName = ref<string>('');
const formType = ref<number>(0); // 0: Standart, 1: Giyim
const loading = ref<boolean>(false);
const submitting = ref<boolean>(false);
const editingId = ref<number | null>(null); // Düzenleme modunda mı? (Varsa ID tutar)

// Kategori Türleri
const categoryTypes: CategoryTypeOption[] = [
    { value: 0, label: 'Standart (Genel)' },
    { value: 1, label: 'Giyim / Tekstil (Beden & Renk)' }
];

// Mod kontrolü (Ekleme mi Güncelleme mi?)
const isEditing = computed<boolean>(() => editingId.value !== null);

// Kategorileri Getir
const fetchCategories = async () => {
    loading.value = true;
    try {
        const res = await api.getCategories();
        // API yanıtı array mi kontrolü ve Cast işlemi
        const data = Array.isArray(res) ? res : (res as any).data || [];
        categories.value = data as Category[];
    } catch (error) {
        toast.error('Kategoriler yüklenirken hata oluştu.');
    } finally {
        loading.value = false;
    }
};

// Formu Sıfırla / İptal Et
const resetForm = () => {
    editingId.value = null;
    formName.value = '';
    formType.value = 0;
};

// Form Gönderimi (Ekle VEYA Güncelle)
const handleSubmit = async () => {
    if (!formName.value.trim()) return;

    submitting.value = true;
    try {
        const payload: CategoryPayload = {
            name: formName.value,
            type: Number(formType.value)
        };

        if (isEditing.value && editingId.value !== null) {
            // --- GÜNCELLEME İŞLEMİ ---
            payload.id = editingId.value;

            // DÖNÜŞÜM: Gelen cevabı zorla Category tipine çeviriyoruz (Type Assertion)
            // Backend'in 'type' alanını döndürdüğünden emin olmalıyız.
            const updatedCat = (await api.updateCategory(editingId.value, payload)) as Category;
            
            // Listeyi güncelle
            const index = categories.value.findIndex(c => c.id === editingId.value);
            if (index !== -1) {
                // Burada backend'den 'type' alanı dönmezse bile UI'da bozulmaması için
                // payload'daki type bilgisini merge ediyoruz.
                categories.value[index] = { ...updatedCat, type: payload.type };
            }

            toast.success('Kategori güncellendi.');
        } else {
            // --- EKLEME İŞLEMİ ---
            // DÖNÜŞÜM: Gelen cevabı Category olarak işaretliyoruz
            const newCat = (await api.createCategory(payload)) as Category;
            
            // Backend bazen create işleminde sadece ID döner, eksik field varsa tamamlayalım
            const safeCat: Category = {
                ...newCat,
                name: newCat.name || payload.name,
                type: newCat.type ?? payload.type, // type 0 olabilir, o yüzden ?? kullandık
                productCount: 0
            };
            
            categories.value.push(safeCat);
            toast.success('Kategori eklendi.');
        }

        resetForm();
    } catch (error: any) {
        const msg = error.response?.data?.message || 'İşlem başarısız.';
        if (error.response?.data?.errors) {
            const errs = Object.values(error.response.data.errors).flat();
            (errs as string[]).forEach(e => toast.error(e));
        } else {
            toast.error(msg);
        }
    } finally {
        submitting.value = false;
    }
};

// Düzenleme Modunu Başlat
const startEdit = (category: Category) => {
    editingId.value = category.id;
    formName.value = category.name;
    formType.value = category.type; 
    
    window.scrollTo({ top: 0, behavior: 'smooth' });
};

// Kategori Sil
const deleteCategory = async (id: number) => {
    if (!confirm('Bu kategoriyi silmek istediğinize emin misiniz?')) return;

    try {
        await api.deleteCategory(id);
        categories.value = categories.value.filter(c => c.id !== id);
        toast.success('Kategori silindi.');
        
        if (editingId.value === id) resetForm();
    } catch (error: any) {
        const msg = error.response?.data?.message || 'Silme işlemi başarısız. (Ürün var mı?)';
        toast.error(msg);
    }
};

const getTypeName = (typeVal: number) => typeVal === 1 ? 'Giyim' : 'Standart';

onMounted(() => {
    fetchCategories();
});
</script>

<template>
    <div class="space-y-6">
        <div>
            <h3 class="text-lg font-bold text-white">Kategori Yönetimi</h3>
            <p class="text-gray-500 text-sm mt-1">
                Ürünlerinizi gruplandırın. 
                <span class="text-indigo-400">"Giyim"</span> türünü seçerseniz ürünlerde beden ve renk özellikleri aktif olur.
            </p>
        </div>

        <div class="bg-[#151521] border border-gray-700 p-4 rounded-lg transition-colors" :class="{'border-indigo-500/50': isEditing}">
            <div class="flex items-center justify-between mb-2" v-if="isEditing">
                <span class="text-xs font-bold text-indigo-400 uppercase tracking-wider">Düzenleme Modu</span>
                <button @click="resetForm" class="text-xs text-gray-400 hover:text-white underline">Vazgeç</button>
            </div>

            <div class="flex flex-col sm:flex-row gap-4">
                <input 
                    v-model="formName" 
                    type="text" 
                    placeholder="Kategori adı..." 
                    class="flex-[2] bg-[#1E1E2D] border border-gray-600 text-white text-sm rounded-lg focus:ring-indigo-500 focus:border-indigo-500 block w-full p-2.5"
                    @keyup.enter="handleSubmit"
                >
                
                <select 
                    v-model="formType"
                    class="flex-1 bg-[#1E1E2D] border border-gray-600 text-white text-sm rounded-lg focus:ring-indigo-500 focus:border-indigo-500 block w-full p-2.5"
                >
                    <option v-for="type in categoryTypes" :key="type.value" :value="type.value">
                        {{ type.label }}
                    </option>
                </select>

                <button 
                    @click="handleSubmit" 
                    :disabled="submitting || !formName.trim()"
                    class="font-medium rounded-lg text-sm px-5 py-2.5 transition-colors whitespace-nowrap text-white disabled:opacity-50"
                    :class="isEditing ? 'bg-indigo-500 hover:bg-indigo-600' : 'bg-green-600 hover:bg-green-700'"
                >
                    {{ submitting ? 'İşleniyor...' : (isEditing ? 'Güncelle' : 'Ekle') }}
                </button>
            </div>
        </div>

        <div class="bg-[#151521] rounded-lg border border-gray-700 overflow-hidden">
            <div v-if="loading" class="p-4 text-center text-gray-500 text-sm">Yükleniyor...</div>
            
            <div v-else-if="categories.length === 0" class="p-8 text-center text-gray-500 text-sm">
                Henüz hiç kategori yok.
            </div>

            <ul v-else class="divide-y divide-gray-700">
                <li v-for="category in categories" :key="category.id" class="flex items-center justify-between p-4 hover:bg-[#1E1E2D] transition-colors">
                    <div class="flex items-center gap-3">
                        <span class="text-white font-medium">{{ category.name }}</span>
                        
                        <span 
                            class="text-[10px] px-2 py-0.5 rounded border"
                            :class="category.type === 1 
                                ? 'text-indigo-400 border-indigo-900 bg-indigo-900/20' 
                                : 'text-gray-400 border-gray-700 bg-gray-800'"
                        >
                            {{ getTypeName(category.type) }}
                        </span>

                        <span class="text-xs text-gray-500 ml-1">({{ category.productCount }} Ürün)</span>
                    </div>

                    <div class="flex items-center gap-2">
                        <button 
                            @click="startEdit(category)" 
                            class="text-blue-400 hover:text-blue-300 text-sm font-medium transition-colors px-2 py-1"
                        >
                            Düzenle
                        </button>

                        <button 
                            @click="deleteCategory(category.id)" 
                            class="text-red-400 hover:text-red-300 text-sm font-medium transition-colors px-2 py-1"
                        >
                            Sil
                        </button>
                    </div>
                </li>
            </ul>
        </div>
        
        <p class="text-xs text-gray-600 mt-2">* İçinde ürün bulunan kategoriler silinemez.</p>
    </div>
</template>