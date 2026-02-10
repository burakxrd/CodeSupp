<script setup lang="ts">
import { ref, watch, onUnmounted } from 'vue';
import { useToast } from 'vue-toastification';
import { getImageUrl } from '../../utils/urlHelper'; 
// --- TYPES & PROPS ---
interface Props {
    // Backend'den gelen mevcut resim yolu (Edit modu)
    initialImage?: string | null;
}

const props = withDefaults(defineProps<Props>(), {
    initialImage: null
});

// --- EMITS ---
// 'change': Yeni dosya seçildiğinde File, silindiğinde null döner
const emit = defineEmits<{
    (e: 'change', file: File | null): void;
}>();

// --- STATE ---
const toast = useToast();
// DOM Element Ref (HTMLInputElement olduğunu belirtiyoruz)
const fileInput = ref<HTMLInputElement | null>(null);
const previewUrl = ref<string | null>(null);
const fileName = ref<string>('');

// --- ACTIONS ---

const resetState = () => {
    previewUrl.value = null;
    fileName.value = '';
};

// Dosya Seçimi Handler'ı
const handleFileChange = (event: Event) => {
    // Event target'ı HTMLInputElement olarak cast ediyoruz
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    
    if (file) {
        // Validasyon: Max 5MB
        if (file.size > 5 * 1024 * 1024) {
            toast.warning("Dosya boyutu 5MB'dan büyük olamaz!");
            target.value = ''; // Input'u temizle
            return;
        }

        // 1. Dosya adını sakla
        fileName.value = file.name;

        // 2. Bellek temizliği (Önceki blob varsa sil)
        if (previewUrl.value && previewUrl.value.startsWith('blob:')) {
            URL.revokeObjectURL(previewUrl.value);
        }
        
        // 3. Yeni önizleme oluştur
        previewUrl.value = URL.createObjectURL(file);

        // 4. Parent'a dosyayı gönder
        emit('change', file);
    }
};

// Resmi tamamen kaldır (Hem önizlemeyi hem seçimi)
const removeImage = (e?: Event) => {
    // Parent click event'ini durdur (TriggerInput çalışmasın diye)
    if(e) e.stopPropagation();

    // Bellek temizliği
    if (previewUrl.value && previewUrl.value.startsWith('blob:')) {
        URL.revokeObjectURL(previewUrl.value);
    }

    resetState();
    
    // Input'u sıfırla (Aynı dosyayı tekrar seçebilmek için)
    if (fileInput.value) fileInput.value.value = '';

    // Parent'a "Resim silindi" bilgisini gönder (null)
    emit('change', null);
};

const triggerInput = () => {
    // Optional chaining (?.) ile güvenli erişim
    fileInput.value?.click();
};

// --- WATCHERS ---
// Edit modunda resim gelirse göster
watch(() => props.initialImage, (newPath) => {
    // Eğer kullanıcı henüz yeni bir resim seçmediyse (blob yoksa) backend resmini göster
    if (newPath && (!previewUrl.value || !previewUrl.value.startsWith('blob:'))) {
        // getImageUrl string | undefined dönebilir, null ile fallback yapıyoruz
        previewUrl.value = getImageUrl(newPath) || null;
        fileName.value = ''; // Backend'den geliyorsa dosya adı boş kalsın
    } else if (!newPath && !previewUrl.value) {
        // Hem backend boş hem biz seçmedik -> Sıfırla
        resetState();
    }
}, { immediate: true });


// Bileşen yok olurken bellek temizliği (Memory Leak Koruması)
onUnmounted(() => {
    if (previewUrl.value && previewUrl.value.startsWith('blob:')) {
        URL.revokeObjectURL(previewUrl.value);
    }
});
</script>

<template>
    <div class="flex items-start gap-4">
        <div 
            class="w-24 h-24 flex-shrink-0 bg-[#151521] border border-gray-600 rounded-xl overflow-hidden flex items-center justify-center relative group cursor-pointer shadow-lg shadow-black/20 hover:border-indigo-500 transition-colors"
            @click="triggerInput"
        >
            <img v-if="previewUrl" :src="previewUrl" class="w-full h-full object-cover" alt="Ürün Önizleme">
            
            <div v-else class="text-gray-500 flex flex-col items-center p-2 text-center">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mb-1 opacity-50" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
                </svg>
                <span class="text-[9px] font-medium uppercase tracking-wide opacity-50">Resim Seç</span>
            </div>

            <div 
                class="absolute inset-0 bg-black/70 flex flex-col items-center justify-center opacity-0 group-hover:opacity-100 transition-all duration-200"
            >
                <span class="text-white text-xs font-bold mb-2">Değiştir</span>
                
                <button 
                    v-if="previewUrl"
                    @click="removeImage"
                    type="button"
                    class="bg-red-500/20 text-red-400 hover:bg-red-500 hover:text-white p-1.5 rounded-full transition-colors"
                    title="Resmi Kaldır"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                        <path fill-rule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 10-2 0h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clip-rule="evenodd" />
                    </svg>
                </button>
            </div>
        </div>

        <div class="flex-1 min-w-0">
            <label class="block text-sm font-medium text-gray-400 mb-1.5">
                Ürün Görseli <span class="text-gray-600 text-xs font-normal ml-1">(İsteğe bağlı)</span>
            </label>
            
            <input 
                type="file" 
                ref="fileInput" 
                @change="handleFileChange" 
                accept="image/png, image/jpeg, image/jpg, image/webp" 
                class="hidden"
            >
            
            <div class="flex gap-2">
                <button 
                    @click="triggerInput" 
                    type="button" 
                    class="flex-1 text-sm text-gray-300 bg-[#151521] hover:bg-[#1E1E2D] border border-dashed border-gray-600 hover:border-indigo-500 hover:text-indigo-400 rounded-lg px-4 py-2.5 text-left transition-all truncate group"
                >
                    <span v-if="fileName" class="text-white">{{ fileName }}</span>
                    <span v-else class="text-gray-500 group-hover:text-indigo-400">Bilgisayardan dosya seç...</span>
                </button>

                <button 
                    v-if="previewUrl"
                    @click="removeImage"
                    type="button"
                    class="px-3 py-2 bg-red-500/10 border border-red-500/20 rounded-lg text-red-400 hover:bg-red-500 hover:text-white transition-colors"
                    title="Resmi Temizle"
                >
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                        <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                    </svg>
                </button>
            </div>
            
            <p class="text-[11px] text-gray-500 mt-2 leading-relaxed">
                <span class="text-indigo-400 font-bold">İpucu:</span> Yüksek kaliteli ürün görselleri satışları artırır. 
                Max 5MB (JPG, PNG, WEBP).
            </p>
        </div>
    </div>
</template>