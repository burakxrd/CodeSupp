import axios, { 
    type AxiosInstance, 
    type InternalAxiosRequestConfig, 
    type AxiosResponse, 
    AxiosError 
} from 'axios';
import router from '../router'; 
import { API_ROUTES } from '../constants/apiRoutes'; 

// --- CONFIGURATION ---
const baseURL = '/api'; 

const apiClient: AxiosInstance = axios.create({
    baseURL,
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    },
    timeout: 15000 
});

// --- HELPER: Hata Mesajı Ayrıştırıcı ---
// Gelen hatanın tipini 'any' olarak alıyoruz çünkü Axios hatası dışında sistemsel hatalar da olabilir
const parseErrorMessage = (error: any): string => {
    if (!error.response) return 'Sunucuya erişilemiyor. İnternet bağlantınızı kontrol edin.';
    
    const data = error.response.data;
    
    if (data?.message) return data.message;
    
    // Backend'den gelen validation hataları genelde { errors: { field: ['Hata'] } } yapısındadır
    if (data?.errors && typeof data.errors === 'object') {
        const firstErrorKey = Object.keys(data.errors)[0];
        // @ts-ignore: Dinamik key erişimi için
        return data.errors[firstErrorKey][0] || 'Validasyon hatası oluştu.';
    }

    return error.message || 'Beklenmedik bir hata oluştu.';
};

// --- REQUEST INTERCEPTOR ---
apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        return config;
    },
    (error: any) => Promise.reject(error)
);

// --- RESPONSE INTERCEPTOR ---
apiClient.interceptors.response.use(
    (response: AxiosResponse) => response, 
    async (error: AxiosError) => {
        // TypeScript'i kandırmak için config objesini genişletilmiş bir tip gibi görüyoruz
        const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
        
        // --- 1. AUTO-RETRY MEKANİZMASI ---
        if ((!error.response || error.response.status === 503) && originalRequest && !originalRequest._retry) {
            originalRequest._retry = true;
            console.warn("Ağ hatası algılandı, istek tekrar deneniyor...");
            await new Promise(resolve => setTimeout(resolve, 1000));
            return apiClient(originalRequest);
        }

        let message = parseErrorMessage(error);
        const status = error.response ? error.response.status : null;

        // --- 2. GÜVENLİK VE YETKİ KONTROLLERİ ---

        // 401: Unauthorized
        if (status === 401 && originalRequest) {
            const url = originalRequest.url || '';
            const isAuthRequest = url.includes(API_ROUTES.AUTH.LOGIN) || 
                                  url.includes(API_ROUTES.AUTH.ME);

            if (!isAuthRequest) {
                if (router.currentRoute.value.name !== 'login') {
                    router.push({
                        name: 'login',
                        query: { redirect: router.currentRoute.value.fullPath }
                    });
                }
                message = 'Oturum süreniz doldu. Lütfen tekrar giriş yapın.';
            }
        }
        
        else if (status === 403) {
            message = 'Bu işlem için yetkiniz bulunmamaktadır.';
        }
        
        else if (status && status >= 500) {
            message = 'Sunucu kaynaklı bir sorun oluştu. Lütfen daha sonra tekrar deneyin.';
            console.error("Critical Server Error:", error);
        }

        // --- 3. GLOBAL HATA BİLDİRİMİ ---
        // Auth istekleri hariç diğer hataları global event olarak fırlat (Toast mesajı göstermek için vs.)
        if (originalRequest) {
            const url = originalRequest.url || '';
            const isAuthRequest = url.includes(API_ROUTES.AUTH.LOGIN) || 
                                  url.includes(API_ROUTES.AUTH.ME);

            if (status !== 401 || !isAuthRequest) {
                 if(status !== 401) {
                    const event = new CustomEvent('api-error', { detail: message });
                    window.dispatchEvent(event);
                 }
            }
        }

        // Error objesine customMessage ekleyip fırlatıyoruz
        // (error as any) diyerek TS kontrolünü bypass ediyoruz çünkü standart Error tipinde bu field yok
        (error as any).customMessage = message;
        return Promise.reject(error);
    }
);

export default apiClient;