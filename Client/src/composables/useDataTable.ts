// src/composables/useDataTable.ts

import { ref, watch, onUnmounted, type Ref } from 'vue';
import { useToast } from 'vue-toastification';

// --- TİP TANIMLAMALARI ---

// Backend'den dönen standart sayfalama yapısı
export interface PaginatedResult<T> {
    items: T[];
    totalCount: number;
    totalPages: number;
    pageNumber: number;
    pageSize: number;
}

// Composable'a gönderilecek parametrelerin tipi
interface UseDataTableOptions<T> {
    apiFetch: (params: any) => Promise<PaginatedResult<T> | T[]>; // Hem sayfalama hem düz array dönebilir
    apiDelete: (id: number | string) => Promise<any>;
    filters?: Ref<Record<string, any>>; // Ref içinde bir obje
    pageSizeInit?: number;
}

// --- COMPOSABLE ---

// <T> Generic tipi sayesinde bu fonksiyonu hangi modelle kullanırsan (Product, Purchase vb.) o tipi tanır.
export function useDataTable<T>({ 
    apiFetch, 
    apiDelete, 
    filters = ref({}), 
    pageSizeInit = 10 
}: UseDataTableOptions<T>) {
    
    const toast = useToast();
    
    // --- STATE ---
    // items ref'i T array tipinde olacak (Örn: Product[])
    const items = ref<T[]>([]) as Ref<T[]>;
    const isLoading = ref(false);
    const searchQuery = ref('');
    
    // Sayfalama
    const currentPage = ref(1);
    const totalPages = ref(1);
    const totalCount = ref(0);
    const pageSize = ref(pageSizeInit);

    // Debounce Timer Referansı (NodeJS.Timeout veya number olabilir)
    let searchTimeout: ReturnType<typeof setTimeout> | null = null;

    // --- CORE ACTIONS ---
    
    // 1. Veri Çekme
    const fetchData = async (page = 1) => {
        isLoading.value = true;
        try {
            const params = {
                page, 
                pageSize: pageSize.value,
                search: searchQuery.value,
                ...filters.value 
            };

            const res = await apiFetch(params);

            // Gelen veri PaginatedResult yapısında mı kontrol ediyoruz
            if (res && 'items' in res && 'totalCount' in res) {
                // Backend sayfalı veri döndü
                items.value = res.items;
                totalCount.value = res.totalCount;
                totalPages.value = res.totalPages;
                currentPage.value = res.pageNumber;
            } else {
                // Backend düz liste döndü
                items.value = Array.isArray(res) ? res : [];
                totalCount.value = items.value.length;
            }
        } catch (err) {
            console.error("Tablo verisi yüklenirken hata:", err);
            toast.error("Veriler yüklenirken bir sorun oluştu.");
            items.value = [];
        } finally {
            isLoading.value = false;
        }
    };

    // 2. Filtreleri İzleme
    watch(filters, () => {
        currentPage.value = 1;
        fetchData(1);
    }, { deep: true });

    // 3. Arama (Modern Watch + Debounce)
    watch(searchQuery, () => {
        if (searchTimeout) clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            currentPage.value = 1; 
            fetchData(1);
        }, 500);
    });

    // 4. Sayfa Değiştirme
    const changePage = (p: number) => {
        if (p >= 1 && p <= totalPages.value) {
            fetchData(p);
        }
    };

    // 5. Silme İşlemi
    const handleDelete = async (id: number | string, confirmMsg: string | null = null) => {
        
        if (confirmMsg && !window.confirm(confirmMsg)) return false;

        try {
            await apiDelete(id);
            toast.success("Kayıt başarıyla silindi.");
            
            // Eğer sayfadaki son kaydı sildiysek bir önceki sayfaya git
            if (items.value.length === 1 && currentPage.value > 1) {
                fetchData(currentPage.value - 1);
            } else {
                fetchData(currentPage.value);
            }
            return true; 
        } catch (err: any) {
            console.error(err);
            const msg = err.response?.data?.message || "Silme işlemi başarısız.";
            toast.error(msg);
            return false; 
        }
    };

    // 6. Cleanup (Memory Leak Önlemi)
    onUnmounted(() => {
        if (searchTimeout) clearTimeout(searchTimeout);
    });

    return {
        items,
        isLoading,
        searchQuery,
        currentPage,
        totalPages,
        totalCount,
        pageSize,
        fetchData,
        changePage,
        handleDelete
    };
}