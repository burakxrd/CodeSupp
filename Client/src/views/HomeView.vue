<script setup lang="ts">
import { ref } from 'vue';

// --- TYPES ---
interface Feature {
  title: string;
  desc: string;
  icon: string;
}

// --- DATA ---
const features: Feature[] = [
  {
    title: 'Instagram Entegrasyonu',
    desc: 'DM ve yorumlardan gelen siparişleri tek panelde topla, müşteriye anında dönüş yap.',
    icon: 'M13 10V3L4 14h7v7l9-11h-7z'
  },
  {
    title: 'Akıllı Stok Yönetimi',
    desc: 'Hangi ürünün ne kadar kaldığını anlık gör, kritik stok uyarıları al, asla yok satma.',
    icon: 'M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4' 
  },
  {
    title: 'Finansal Analiz',
    desc: 'Günlük ciro, net kâr ve gider takibi ile işletmenin nabzını tut. Rakamlarla yönet.',
    icon: 'M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z' // Currency icon
  },
  {
    title: 'AI Destekli Asistan',
    desc: 'Müşteri sorularına otomatik yanıtlar oluştur, operasyonel yükünü yapay zeka ile hafiflet.',
    icon: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z' // Bulb/AI icon
  }
];

const mobileMenuOpen = ref<boolean>(false);

const toggleMobileMenu = (): void => {
  mobileMenuOpen.value = !mobileMenuOpen.value;
};
</script>

<template>
  <div class="flex flex-col min-h-screen bg-[#151521] text-gray-300 font-sans selection:bg-indigo-500 selection:text-white">
    
    <div class="fixed inset-0 overflow-hidden pointer-events-none">
        <div class="absolute top-[-10%] left-[-10%] w-[500px] h-[500px] bg-indigo-600/10 rounded-full blur-[120px]"></div>
        <div class="absolute bottom-[-10%] right-[-10%] w-[500px] h-[500px] bg-purple-600/10 rounded-full blur-[120px]"></div>
        <div class="absolute inset-0 bg-[url('https://grainy-gradients.vercel.app/noise.svg')] opacity-5"></div>
    </div>

    <header class="fixed top-0 left-0 right-0 z-50 bg-[#151521]/80 backdrop-blur-lg border-b border-gray-800">
        <nav class="container mx-auto px-4 sm:px-6 lg:px-8 flex justify-between items-center h-20">
            <a href="#" class="flex items-center gap-2 group">
                <div class="w-8 h-8 rounded-lg bg-gradient-to-tr from-indigo-500 to-purple-600 flex items-center justify-center shadow-lg shadow-indigo-500/20 group-hover:scale-110 transition-transform">
                    <span class="text-white font-bold text-lg">C</span>
                </div>
                <span class="text-2xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-gray-400">CodeSupp</span>
            </a>

            <div class="hidden md:flex items-center space-x-8">
                <a href="#features" class="text-sm font-medium text-gray-400 hover:text-white transition-colors">Özellikler</a>
                <a href="#solutions" class="text-sm font-medium text-gray-400 hover:text-white transition-colors">Çözümler</a>
                <a href="#pricing" class="text-sm font-medium text-gray-400 hover:text-white transition-colors">Fiyatlandırma</a>
            </div>

            <div class="hidden md:flex items-center space-x-4">
                <router-link to="/login" class="text-sm font-medium text-gray-300 hover:text-white transition-colors">Giriş Yap</router-link>
                <router-link to="/register" class="bg-indigo-600 hover:bg-indigo-500 text-white text-sm font-bold py-2.5 px-5 rounded-xl transition-all shadow-lg shadow-indigo-600/25 hover:shadow-indigo-600/40 active:scale-95">
                    Hemen Başla
                </router-link>
            </div>

            <button @click="toggleMobileMenu" class="md:hidden text-gray-300 hover:text-white focus:outline-none">
                <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16m-7 6h7" />
                </svg>
            </button>
        </nav>

        <div v-if="mobileMenuOpen" class="md:hidden bg-[#1E1E2D] border-b border-gray-800 p-4 absolute w-full">
            <div class="flex flex-col space-y-4">
                <a href="#features" class="text-gray-400 hover:text-white">Özellikler</a>
                <router-link to="/login" class="text-gray-400 hover:text-white">Giriş Yap</router-link>
                <router-link to="/register" class="text-indigo-400 font-bold">Hemen Başla</router-link>
            </div>
        </div>
    </header>

    <main class="flex-grow relative z-10">
        
        <section class="pt-32 pb-20 lg:pt-48 lg:pb-32 px-4 relative">
            <div class="container mx-auto text-center max-w-5xl">
                
                <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 text-xs font-semibold uppercase tracking-wide mb-8 animate-fade-in-up">
                    <span class="w-2 h-2 rounded-full bg-indigo-500 animate-pulse"></span>
                    Yeni: AI Modülü Yayında
                </div>

                <h1 class="text-5xl md:text-7xl font-extrabold text-white mb-8 tracking-tight leading-tight">
                    Instagram Kaosunu <br>
                    <span class="text-transparent bg-clip-text bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400">Otomasyona</span> Dönüştür.
                </h1>

                <p class="text-xl text-gray-400 max-w-2xl mx-auto mb-12 leading-relaxed">
                    DM'lerden sipariş almayı bırakın, profesyonel sisteme geçin. CodeSupp ile stok takibi, sipariş yönetimi ve finansal raporlar tek panelde.
                </p>

                <div class="flex flex-col sm:flex-row items-center justify-center gap-4 mb-20">
                    <router-link to="/register" class="w-full sm:w-auto bg-white text-[#151521] hover:bg-gray-100 font-bold py-4 px-8 rounded-xl transition-all shadow-xl hover:shadow-2xl hover:-translate-y-1">
                        14 Gün Ücretsiz Dene
                    </router-link>
                    <a href="#how-it-works" class="w-full sm:w-auto px-8 py-4 rounded-xl border border-gray-700 hover:border-gray-500 text-gray-300 hover:text-white font-medium transition-all flex items-center justify-center gap-2">
                        <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clip-rule="evenodd" />
                        </svg>
                        Nasıl Çalışır?
                    </a>
                </div>

                <div class="relative mx-auto max-w-5xl group">
                    <div class="absolute -inset-1 bg-gradient-to-r from-indigo-500 to-purple-600 rounded-2xl blur opacity-20 group-hover:opacity-40 transition duration-1000"></div>
                    
                    <div class="relative bg-[#1E1E2D] border border-gray-800 rounded-xl shadow-2xl overflow-hidden">
                        <div class="bg-[#151521] border-b border-gray-800 px-4 py-3 flex items-center gap-2">
                            <div class="w-3 h-3 rounded-full bg-red-500/50"></div>
                            <div class="w-3 h-3 rounded-full bg-yellow-500/50"></div>
                            <div class="w-3 h-3 rounded-full bg-green-500/50"></div>
                        </div>
                        <div class="p-6 grid grid-cols-12 gap-6 h-[300px] md:h-[500px] opacity-80">
                            <div class="col-span-2 hidden md:block space-y-3 border-r border-gray-800 pr-4">
                                <div class="h-8 bg-gray-800 rounded w-3/4"></div>
                                <div class="h-4 bg-gray-800/50 rounded w-full"></div>
                                <div class="h-4 bg-gray-800/50 rounded w-5/6"></div>
                                <div class="h-4 bg-gray-800/50 rounded w-4/5"></div>
                            </div>
                            <div class="col-span-12 md:col-span-10 space-y-6">
                                <div class="flex justify-between">
                                    <div class="h-8 bg-gray-800 rounded w-1/3"></div>
                                    <div class="h-8 bg-indigo-600/20 rounded w-24"></div>
                                </div>
                                <div class="grid grid-cols-3 gap-4">
                                    <div class="h-32 bg-gray-800/30 rounded-lg border border-gray-800 flex items-end p-4 gap-2">
                                        <div class="w-full bg-indigo-500/20 h-[40%] rounded-t"></div>
                                        <div class="w-full bg-indigo-500/40 h-[70%] rounded-t"></div>
                                        <div class="w-full bg-indigo-500/60 h-[50%] rounded-t"></div>
                                        <div class="w-full bg-indigo-500 h-[80%] rounded-t"></div>
                                    </div>
                                    <div class="col-span-2 h-32 bg-gray-800/30 rounded-lg border border-gray-800 p-4 space-y-2">
                                        <div class="h-2 bg-gray-700 rounded w-full"></div>
                                        <div class="h-2 bg-gray-700 rounded w-5/6"></div>
                                        <div class="h-2 bg-gray-700 rounded w-4/5"></div>
                                    </div>
                                </div>
                                <div class="space-y-3">
                                    <div v-for="i in 4" :key="i" class="h-12 bg-gray-800/20 rounded border border-gray-800/50 flex items-center px-4 justify-between">
                                        <div class="w-8 h-8 rounded-full bg-gray-700"></div>
                                        <div class="h-2 bg-gray-700 w-1/3"></div>
                                        <div class="h-2 bg-green-500/50 w-16"></div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="absolute inset-0 flex items-center justify-center bg-black/40 backdrop-blur-[2px]">
                            <p class="text-white font-bold text-xl md:text-2xl bg-black/50 px-6 py-3 rounded-xl border border-white/10 backdrop-blur-md">
                                Tüm süreçlerin parmaklarının ucunda 👋
                            </p>
                        </div>
                    </div>
                </div>

                <div class="mt-20 pt-10 border-t border-gray-800/50">
                    <p class="text-sm text-gray-500 font-medium mb-6 uppercase tracking-widest">Sayılarla CodeSupp</p>
                    <div class="grid grid-cols-2 md:grid-cols-4 gap-8">
                        <div>
                            <div class="text-3xl font-bold text-white mb-1">₺10M+</div>
                            <div class="text-sm text-gray-500">Yönetilen Ciro</div>
                        </div>
                        <div>
                            <div class="text-3xl font-bold text-white mb-1">500+</div>
                            <div class="text-sm text-gray-500">Aktif Satıcı</div>
                        </div>
                        <div>
                            <div class="text-3xl font-bold text-white mb-1">%99.9</div>
                            <div class="text-sm text-gray-500">Uptime</div>
                        </div>
                        <div>
                            <div class="text-3xl font-bold text-white mb-1">7/24</div>
                            <div class="text-sm text-gray-500">Destek</div>
                        </div>
                    </div>
                </div>

            </div>
        </section>

        <section id="features" class="py-24 bg-[#1A1A27] relative">
            <div class="container mx-auto px-4">
                <div class="text-center max-w-3xl mx-auto mb-16">
                    <h2 class="text-3xl md:text-4xl font-bold text-white mb-4">İhtiyacınız olan her şey tek kutuda.</h2>
                    <p class="text-gray-400">Excel dosyalarıyla boğuşmayı bırakın. Modern e-ticaret için modern araçlar kullanın.</p>
                </div>

                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                    <div v-for="(feature, idx) in features" :key="idx" 
                        class="bg-[#151521] p-8 rounded-2xl border border-gray-800 hover:border-indigo-500/50 transition-colors group relative overflow-hidden">
                        
                        <div class="absolute top-0 right-0 w-24 h-24 bg-indigo-600/10 rounded-bl-full -mr-4 -mt-4 transition-transform group-hover:scale-150"></div>
                        
                        <div class="w-12 h-12 bg-[#1E1E2D] rounded-lg flex items-center justify-center mb-6 group-hover:bg-indigo-600 transition-colors">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-indigo-400 group-hover:text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="feature.icon" />
                            </svg>
                        </div>
                        <h3 class="text-xl font-bold text-white mb-3">{{ feature.title }}</h3>
                        <p class="text-gray-400 text-sm leading-relaxed">
                            {{ feature.desc }}
                        </p>
                    </div>
                </div>
            </div>
        </section>

        <section class="py-24 relative overflow-hidden">
            <div class="container mx-auto px-4 relative z-10 text-center">
                <div class="bg-gradient-to-r from-indigo-900/50 to-purple-900/50 border border-indigo-500/30 rounded-3xl p-12 md:p-20 relative overflow-hidden">
                    <div class="absolute top-0 right-0 w-64 h-64 bg-indigo-500/20 rounded-full blur-[80px]"></div>
                    <div class="absolute bottom-0 left-0 w-64 h-64 bg-purple-500/20 rounded-full blur-[80px]"></div>
                    
                    <h2 class="text-3xl md:text-5xl font-bold text-white mb-6">İşinizi büyütmeye hazır mısınız?</h2>
                    <p class="text-lg text-gray-300 max-w-2xl mx-auto mb-10">
                        Kredi kartı gerekmez. Kurulum gerekmez. Hesabınızı oluşturun ve 2 dakika içinde satışlarınızı yönetmeye başlayın.
                    </p>
                    <router-link to="/register" class="inline-block bg-white text-indigo-900 font-bold py-4 px-10 rounded-xl hover:bg-gray-100 transition-transform hover:-translate-y-1 shadow-2xl">
                        Ücretsiz Hesabını Oluştur
                    </router-link>
                    <p class="mt-4 text-sm text-gray-500">14 gün ücretsiz deneme • İptal edilebilir</p>
                </div>
            </div>
        </section>
        
    </main>
    
    <footer class="bg-[#0f0f18] border-t border-gray-800 pt-16 pb-8">
        <div class="container mx-auto px-4">
            <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-8 mb-12">
                <div class="col-span-2 lg:col-span-2">
                    <a href="#" class="text-2xl font-bold text-white mb-4 block">CodeSupp</a>
                    <p class="text-gray-500 text-sm max-w-xs mb-6">
                        Instagram satıcıları için geliştirilmiş, yeni nesil operasyon yönetim platformu.
                    </p>
                    <div class="flex space-x-4">
                        <a href="#" class="text-gray-500 hover:text-white transition-colors">Twitter</a>
                        <a href="#" class="text-gray-500 hover:text-white transition-colors">Instagram</a>
                        <a href="#" class="text-gray-500 hover:text-white transition-colors">LinkedIn</a>
                    </div>
                </div>
                
                <div>
                    <h4 class="text-white font-bold mb-4">Ürün</h4>
                    <ul class="space-y-2 text-sm text-gray-500">
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Özellikler</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Fiyatlandırma</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Entegrasyonlar</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Yol Haritası</a></li>
                    </ul>
                </div>

                <div>
                    <h4 class="text-white font-bold mb-4">Şirket</h4>
                    <ul class="space-y-2 text-sm text-gray-500">
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Hakkımızda</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Blog</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Kariyer</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">İletişim</a></li>
                    </ul>
                </div>

                <div>
                    <h4 class="text-white font-bold mb-4">Yasal</h4>
                    <ul class="space-y-2 text-sm text-gray-500">
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Gizlilik Politikası</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">Kullanım Şartları</a></li>
                        <li><a href="#" class="hover:text-indigo-400 transition-colors">KVKK</a></li>
                    </ul>
                </div>
            </div>
            
            <div class="border-t border-gray-800 pt-8 text-center text-sm text-gray-600">
                &copy; 2025 CodeSupp Yazılım A.Ş. Tüm hakları saklıdır.
            </div>
        </div>
    </footer>
  </div>
</template>

<style scoped>
/* Basit animasyonlar için */
@keyframes fade-in-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-fade-in-up {
  animation: fade-in-up 0.8s ease-out forwards;
}
</style>