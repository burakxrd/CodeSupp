<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router'; 
import { useAuthStore } from '../stores/auth';
import { useToast } from "vue-toastification";

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const toast = useToast();

// --- STATE ---
interface LoginForm {
    email: string;
    password: string;
    rememberMe: boolean;
}

const form = reactive<LoginForm>({
    email: '',
    password: '',
    rememberMe: false
});

const isLoading = ref<boolean>(false);
const showPassword = ref<boolean>(false);

// --- LIFECYCLE ---
onMounted(() => {
    // Sayfa açıldığında "Beni Hatırla" kontrolü yap
    const savedEmail = localStorage.getItem('rememberedEmail');
    if (savedEmail) {
        form.email = savedEmail;
        form.rememberMe = true;
    }
});

// --- ACTIONS ---
const togglePassword = (): void => {
    showPassword.value = !showPassword.value;
};

const handleLogin = async (): Promise<void> => {
    // 1. Basit Validasyon
    if (!form.email || !form.password) {
        toast.warning("Lütfen email ve şifre alanlarını doldurun.");
        return;
    }

    isLoading.value = true;

    try {
        // 2. Login İsteği
        // authStore.login'in boolean döndürdüğünü varsayıyoruz
        const success = await authStore.login(form.email, form.password);

        if (success) {
            toast.success(`Hoş geldin, ${authStore.user?.fullName || 'Kullanıcı'}! 👋`);
            
            // --- "BENİ HATIRLA" MANTIĞI ---
            if (form.rememberMe) {
                localStorage.setItem('rememberedEmail', form.email);
            } else {
                localStorage.removeItem('rememberedEmail');
            }

            // --- AKILLI YÖNLENDİRME ---
            // query parametresi array de olabilir, string olarak cast ediyoruz
            const redirectPath = (route.query.redirect as string) || '/dashboard';
            router.push(redirectPath);
        } 
    } catch (error) {
        console.error(error);
    } finally {
        // Hata durumunda interceptor toast basıyor, biz sadece loading'i kapatıyoruz.
        isLoading.value = false;
    }
};
</script>

<template>
    <div class="min-h-screen flex items-center justify-center bg-[#151521] px-4 relative overflow-hidden">
        
        <div class="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none">
            <div class="absolute -top-[10%] -left-[10%] w-[40%] h-[40%] bg-purple-600/20 rounded-full blur-[100px]"></div>
            <div class="absolute top-[20%] -right-[10%] w-[30%] h-[30%] bg-blue-600/10 rounded-full blur-[80px]"></div>
        </div>

        <div class="bg-[#1E1E2D] p-8 rounded-2xl shadow-2xl w-full max-w-md border border-gray-800 relative z-10">
            
            <router-link to="/" class="absolute top-4 right-4 text-gray-500 hover:text-white transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
            </router-link>
            
            <div class="text-center mb-8">
                <div class="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-gradient-to-tr from-indigo-500 to-purple-500 mb-4 shadow-lg shadow-indigo-500/30">
                    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1" />
                    </svg>
                </div>
                <h2 class="text-2xl font-bold text-white">Giriş Yap</h2>
                <p class="text-gray-500 text-sm mt-2">Yönetim paneline erişmek için oturum açın</p>
            </div>
            
            <form @submit.prevent="handleLogin" class="space-y-5">

                <div>
                    <label class="block text-sm font-medium text-gray-400 mb-1.5 ml-1">Email Adresi</label>
                    <div class="relative group">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-500 group-focus-within:text-indigo-400 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                            </svg>
                        </div>
                        <input 
                            v-model="form.email" 
                            type="email" 
                            class="w-full bg-[#151521] border border-gray-700 text-white rounded-xl py-3 pl-10 pr-4 outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all placeholder-gray-600"
                            placeholder="ornek@sirket.com"
                            required
                        >
                    </div>
                </div>

                <div>
                    <label class="block text-sm font-medium text-gray-400 mb-1.5 ml-1">Şifre</label>
                    <div class="relative group">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-500 group-focus-within:text-indigo-400 transition-colors" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                            </svg>
                        </div>
                        <input 
                            v-model="form.password" 
                            :type="showPassword ? 'text' : 'password'" 
                            class="w-full bg-[#151521] border border-gray-700 text-white rounded-xl py-3 pl-10 pr-10 outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all placeholder-gray-600"
                            placeholder="••••••••"
                            required
                        >
                        <button type="button" @click="togglePassword" class="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-500 hover:text-gray-300 focus:outline-none">
                            <svg v-if="!showPassword" xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                            </svg>
                            <svg v-else xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                            </svg>
                        </button>
                    </div>
                </div>

                <div class="flex items-center justify-between text-sm">
                    <label class="flex items-center gap-2 cursor-pointer text-gray-400 hover:text-gray-300">
                        <input type="checkbox" v-model="form.rememberMe" class="rounded bg-[#151521] border-gray-700 text-indigo-500 focus:ring-0 focus:ring-offset-0">
                        <span>Beni Hatırla</span>
                    </label>
                    
                    <router-link to="/forgot-password" class="text-indigo-400 hover:text-indigo-300 transition-colors">
                        Şifremi Unuttum?
                    </router-link>
                </div>

                <button 
                    type="submit" 
                    :disabled="isLoading" 
                    class="w-full bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 text-white font-bold py-3.5 rounded-xl transition-all shadow-lg shadow-indigo-500/25 flex justify-center items-center active:scale-95 disabled:opacity-70 disabled:cursor-not-allowed"
                >
                    <span v-if="!isLoading">Giriş Yap</span>
                    <svg v-else class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                </button>
            </form>
            
            <p class="mt-8 text-center text-gray-500 text-sm">
                Henüz hesabın yok mu? 
                <router-link to="/register" class="text-indigo-400 hover:text-indigo-300 font-medium hover:underline">
                    Hemen Kayıt Ol
                </router-link>
            </p>
        </div>
    </div>
</template>