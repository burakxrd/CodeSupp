import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import api from '../services/httpClient';
import { API_ROUTES } from '../constants/apiRoutes';
import router from '../router'; 

// --- TİP TANIMLAMALARI ---

export interface User {
    id: number;
    email: string;
    fullName?: string; 
    [key: string]: any; 
}

export interface RegisterPayload {
    email: string;
    password: string;
    fullName: string;
    [key: string]: any;
}

interface RegisterResult {
    success: boolean;
    message: string;
}

export const useAuthStore = defineStore('auth', () => {
    // --- STATE ---
    const user = ref<User | null>(null);
    const isLoading = ref<boolean>(true); 

    // --- GETTERS ---
    const isAuthenticated = computed(() => !!user.value);

    // --- ACTIONS ---


    async function fetchUser() {
        try {
            isLoading.value = true;
            const response = await api.get(API_ROUTES.AUTH.ME); 
            user.value = response.data;
        } catch (error) {
            user.value = null;
        } finally {
            isLoading.value = false;
        }
    }
    async function logout() {
        try {
            await api.post(API_ROUTES.AUTH.LOGOUT); 
        } catch (error) {
            console.error("Logout failed", error);
        } finally {
            user.value = null;
            router.push({ name: 'login' });
        }
    }

    async function register(payload: RegisterPayload): Promise<RegisterResult> {
        try {
            const response = await api.post(API_ROUTES.AUTH.REGISTER, payload);
            
            if (response.data.user) {
                user.value = response.data.user;
            }
            
            return { 
                success: true, 
                message: response.data.message || 'Kayıt başarılı!' 
            };
        } catch (error: any) {
            return { 
                success: false, 
                message: error.response?.data?.message || 'Kayıt işlemi başarısız.' 
            };
        }
    }


    async function login(email: string, password: string): Promise<boolean> {
        try {
            const response = await api.post(API_ROUTES.AUTH.LOGIN, { email, password });
            
            const userData = response.data.user;

            if (userData) {
                user.value = userData;
                return true;
            }
            return false;
        } catch (error) {
            console.error("Login Hatası:", error);
            return false;
        }
    }

    // --- INIT ---
    fetchUser();

    return { user, isLoading, isAuthenticated, login, logout, register, fetchUser };
});