import { ref, computed, watch } from 'vue';
import { useToast } from 'vue-toastification';
import { useAuthStore } from '../../stores/auth'; 
import { productSchema, defaultProductValues } from '../../schemas/productSchema'; 
import productService from '../../services/productService'; 
import categoryService from '../../services/categoryService'; 

// --- TİP TANIMLAMALARI ---
export interface Category {
    id: number;
    name: string;
    type: number;
    [key: string]: any;
}

export interface ProductFormState {
    id?: number;
    name: string;
    categoryId: number | undefined; 
    type?: number;
    code?: string;
    barcode?: string;
    costPrice: number;
    price: number;
    discountedPrice?: number | undefined; 
    stock: number;
    description?: string;
    shippingType?: number;
    shippingPrice?: number;
    size?: string;
    color?: string;
    // [YENİ] Concurrency Check için gerekli
    rowVersion?: string;
    [key: string]: any;
}

export function useProductForm() {
    const toast = useToast();
    const authStore = useAuthStore();
    
    // --- STATE ---
    const submitting = ref<boolean>(false);
    const loadingCategories = ref<boolean>(false);
    const categories = ref<Category[]>([]);
    const errors = ref<Record<string, string>>({}); 
    const adjustmentReason = ref<string>('Stok Güncellemesi'); 
    const originalStock = ref<number>(0);
    
    // State'i undefined başlatmak için cast ediyoruz
    const form = ref<ProductFormState>({ ...defaultProductValues } as unknown as ProductFormState);
    
    const discountRate = ref<number>(0);

    // --- CONSTANTS ---
    const productTypes = [
        { id: 0, name: '📦 Fiziksel' },
        { id: 1, name: '💻 Dijital' },
        { id: 2, name: '🛠️ Hizmet' }
    ];
    
    const shippingTypes = [
        { id: 0, name: 'Ücretsiz Kargo (Satıcı Öder)' },
        { id: 1, name: 'Alıcı Öder' },
        { id: 2, name: 'Kapıda Ödeme' }
    ];

    // --- COMPUTED LOGIC ---
    const isClothingCategory = computed(() => {
        if (!form.value.categoryId) return false;
        const cat = categories.value.find(c => c.id === form.value.categoryId);
        return cat?.type === 1; // 1: Giyim
    });

    // --- WATCHERS & MATH ---
    watch(isClothingCategory, (val) => {
        if (val) form.value.type = 0;
    });

    const calculateRate = () => {
        const price = Number(form.value.price);
        const discounted = Number(form.value.discountedPrice);

        if (price > 0 && discounted > 0) {
            const rate = ((price - discounted) / price) * 100;
            discountRate.value = parseFloat(rate.toFixed(2));
        } else {
            discountRate.value = 0;
        }
    };

    const calculatePriceFromRate = () => {
        const price = Number(form.value.price);
        
        if (price > 0 && discountRate.value > 0) {
            const discountAmount = (price * discountRate.value) / 100;
            form.value.discountedPrice = parseFloat((price - discountAmount).toFixed(2));
        } else {
            form.value.discountedPrice = undefined;
        }
    };

    watch(() => form.value.price, () => {
        if (discountRate.value > 0) calculatePriceFromRate();
    });

    // --- ACTIONS ---
    const fetchCategories = async () => {
        if (categories.value.length > 0) return;
        try {
            loadingCategories.value = true;
            const data = await categoryService.getCategories(); 
            categories.value = (data as Category[]) || []; 
        } catch (err) {
            toast.error("Kategoriler yüklenemedi.");
        } finally {
            loadingCategories.value = false;
        }
    };

    const initForm = (productData: Partial<ProductFormState> | null = null) => {
        fetchCategories();
        errors.value = {}; 

        if (productData) {
            // Edit
            form.value = { 
                ...defaultProductValues, 
                ...productData,
                description: productData.description || '',
                code: productData.code || '',
                barcode: productData.barcode || '',
                size: productData.size || '',
                color: productData.color || '',
                categoryId: productData.categoryId ?? undefined, 
                costPrice: Number(productData.costPrice || 0),
                price: Number(productData.price || 0),
                discountedPrice: productData.discountedPrice ?? undefined,
                stock: Number(productData.stock || 0),
                shippingType: Number(productData.shippingType || 0),
                // [YENİ] RowVersion'ı al
                rowVersion: productData.rowVersion
            } as ProductFormState;

            originalStock.value = productData.stock || 0;
            adjustmentReason.value = 'Stok Güncellemesi';
            
            calculateRate();
        } else {
            // Yeni
            form.value = { 
                ...defaultProductValues,
                categoryId: undefined, 
                discountedPrice: undefined,
                rowVersion: undefined // Yeni kayıtta yoktur
            } as unknown as ProductFormState;
            
            originalStock.value = 0;
            adjustmentReason.value = 'Açılış Stoğu';
            discountRate.value = 0;
        }
    };

    const validate = (): boolean => {
        const result = productSchema.safeParse(form.value);

        if (!result.success) {
            const fieldErrors = result.error.flatten().fieldErrors;
            const newErrors: Record<string, string> = {};
            for (const [key, messages] of Object.entries(fieldErrors)) {
                if (messages && messages.length > 0) {
                    newErrors[key] = messages[0];
                }
            }
            errors.value = newErrors;
            const firstErrorMessage = Object.values(errors.value)[0];
            if (firstErrorMessage) toast.warning(firstErrorMessage);
            return false;
        }
        errors.value = {}; 
        return true;
    };

    const submitForm = async (selectedImageFile: File | null, onSuccess?: (res: any) => void) => {
        if (!validate()) return;
        try {
            submitting.value = true;
            const formData = new FormData();
            
            if (form.value.id) formData.append('Id', String(form.value.id));
            if (authStore.user?.tenantId) formData.append('TenantId', String(authStore.user.tenantId));

            // [YENİ] RowVersion Ekle (Concurrency Check)
            if (form.value.rowVersion) {
                formData.append('RowVersion', form.value.rowVersion);
            }

            formData.append('Name', form.value.name);
            if (form.value.categoryId) formData.append('CategoryId', String(form.value.categoryId));
            formData.append('Type', '0'); 
            
            if (form.value.code) formData.append('Code', form.value.code);
            if (form.value.barcode) formData.append('Barcode', form.value.barcode);
            
            formData.append('CostPrice', String(form.value.costPrice));
            formData.append('Price', String(form.value.price));
            
            if (form.value.discountedPrice) formData.append('DiscountedPrice', String(form.value.discountedPrice));
            
            formData.append('Stock', String(form.value.stock));
            
            if (form.value.description) formData.append('Description', form.value.description);
            
            formData.append('ShippingType', String(form.value.shippingType || 0));
            formData.append('ShippingPrice', String(form.value.shippingPrice || 0));
            
            if (form.value.size) formData.append('Size', form.value.size);
            if (form.value.color) formData.append('Color', form.value.color);

            if (selectedImageFile) formData.append('image', selectedImageFile);

            let response;
            if (form.value.id) {
                response = await productService.updateProduct(form.value.id, formData);
                toast.success('Ürün güncellendi.');
            } else {
                response = await productService.createProduct(formData);
                toast.success('Ürün oluşturuldu.');
            }
            if (onSuccess) onSuccess(response);
        } catch (err: any) {
            console.error(err);
            // Backend'den 409 Conflict gelirse özel mesaj gösterilebilir,
            // ama genel hata yakalayıcı mesajı gösterecektir.
            const msg = err.response?.data?.message || (err as any).customMessage || "İşlem başarısız.";
            toast.error(msg);
        } finally {
            submitting.value = false;
        }
    };

    return {
        form,
        errors, 
        categories,
        loadingCategories,
        submitting,
        adjustmentReason, 
        originalStock, 
        discountRate, 
        productTypes,
        shippingTypes,
        isClothingCategory,
        initForm,
        submitForm,
        calculateRate,
        calculatePriceFromRate
    };
}