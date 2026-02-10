<script setup lang="ts">
import { formatCurrency, formatDate, formatRelativeTime } from '../../utils/formatters';
import type { Customer } from '../../types'; // Global Customer tipini import et

// --- TYPES ---

// Backend'den gelen Customer objesinin, View tarafında hesaplanmış ek alanlara sahip hali.
// (Eğer backend bu isVip vs. alanlarını dönüyorsa direkt kullanabiliriz, dönmüyorsa computed yapılmalı)
interface CustomerRow extends Customer {
    isVip?: boolean;
    isLoyal?: boolean;
    isNew?: boolean;
    isRisky?: boolean;
    isPlus75?: boolean;
    orderCount?: number;
    totalSpent?: number;
    lastOrderDate?: string;
    // Customer tipinde olmayan ama burada kullanılan diğer alanlar
}

interface Badge {
    label: string;
    class: string;
}

// --- PROPS ---
interface Props {
    customers: CustomerRow[];
    isLoading?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
    customers: () => [],
    isLoading: false
});

// --- EMITS ---
const emit = defineEmits<{
    (e: 'row-click', customer: CustomerRow): void;
    (e: 'edit', customer: CustomerRow): void;
    (e: 'delete', id: number): void;
}>();

// --- HELPER: Statü Rozetlerini Hesapla ---
const getActiveStatuses = (c: CustomerRow): Badge[] => {
    const list: Badge[] = [];
    if (c.isVip) list.push({ label: 'VIP', class: 'bg-purple-900/60 text-purple-200 border-purple-700/50' });
    if (c.isLoyal) list.push({ label: 'SADIK', class: 'bg-blue-900/60 text-blue-200 border-blue-700/50' });
    if (c.isNew) list.push({ label: 'YENİ', class: 'bg-green-900/60 text-green-200 border-green-700/50' });
    if (c.isRisky) list.push({ label: 'RİSKLİ', class: 'bg-orange-900/60 text-orange-200 border-orange-700/50' });
    if (c.isPlus75) list.push({ label: '+75', class: 'bg-red-900/60 text-red-200 border-red-700/50' });
    return list;
};

// --- Renk Üretici (Deterministic) ---
const getAvatarColor = (name?: string): string => {
    if (!name) return 'bg-gray-600 text-gray-200'; // İsimsizse gri

    const colors = [
        'bg-red-500/20 text-red-400 border-red-500/30',
        'bg-orange-500/20 text-orange-400 border-orange-500/30',
        'bg-amber-500/20 text-amber-400 border-amber-500/30',
        'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
        'bg-lime-500/20 text-lime-400 border-lime-500/30',
        'bg-green-500/20 text-green-400 border-green-500/30',
        'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
        'bg-teal-500/20 text-teal-400 border-teal-500/30',
        'bg-cyan-500/20 text-cyan-400 border-cyan-500/30',
        'bg-sky-500/20 text-sky-400 border-sky-500/30',
        'bg-blue-500/20 text-blue-400 border-blue-500/30',
        'bg-indigo-500/20 text-indigo-400 border-indigo-500/30',
        'bg-violet-500/20 text-violet-400 border-violet-500/30',
        'bg-purple-500/20 text-purple-400 border-purple-500/30',
        'bg-fuchsia-500/20 text-fuchsia-400 border-fuchsia-500/30',
        'bg-pink-500/20 text-pink-400 border-pink-500/30',
        'bg-rose-500/20 text-rose-400 border-rose-500/30'
    ];

    let hash = 0;
    for (let i = 0; i < name.length; i++) {
        hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }

    const index = Math.abs(hash) % colors.length;
    return colors[index];
};
</script>

<template>
    <div class="flex-1 overflow-auto relative custom-scrollbar">
        <div v-if="isLoading" class="p-10 text-center text-gray-500">
            <div class="animate-pulse flex flex-col items-center gap-3">
                <div class="h-4 bg-gray-700 rounded w-1/4"></div>
                <div class="h-4 bg-gray-700 rounded w-1/2"></div>
                <div class="h-4 bg-gray-700 rounded w-1/3"></div>
            </div>
        </div>

        <div v-else>
            <table class="w-full text-left border-collapse min-w-[1000px]"> 
                <thead class="sticky top-0 z-20 bg-[#151521] shadow-sm">
                    <tr class="text-gray-400 text-xs font-bold uppercase tracking-wider border-b border-gray-800">
                        <th class="p-4 pl-6">Müşteri</th>
                        <th class="p-4">Statü</th>
                        <th class="p-4 text-right">Sipariş</th>
                        <th class="p-4 text-right">Harcama</th>
                        <th class="p-4">İletişim</th>
                        <th class="p-4">Son Sipariş</th>
                        <th class="p-4 text-right pr-6">İşlemler</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-gray-800">
                    <tr v-for="c in customers" :key="c.id" 
                        @click="$emit('row-click', c)"
                        class="group hover:bg-[#2B2B40] transition-colors cursor-pointer"
                    >
                        <td class="p-4 pl-6">
                            <div class="flex items-center">
                                <div 
                                    class="h-10 w-10 mr-3 flex-shrink-0 rounded-full flex items-center justify-center text-sm font-bold border shadow-sm transition-transform group-hover:scale-105"
                                    :class="getAvatarColor(c.name)"
                                >
                                    {{ c.name ? c.name.charAt(0).toUpperCase() : '?' }}
                                </div>

                                <div class="min-w-0">
                                    <div class="font-bold text-white text-sm group-hover:text-blue-400 transition-colors truncate max-w-[180px]">
                                        {{ c.name || 'İsimsiz' }}
                                    </div>
                                    <div class="text-xs text-indigo-400 font-medium truncate">
                                        {{ c.instagramHandle || '' }}
                                    </div>
                                </div>
                            </div>
                        </td>

                        <td class="p-4">
                            <div class="flex flex-wrap gap-1 items-center">
                                <template v-if="getActiveStatuses(c).length > 0">
                                    <span 
                                        v-for="(s, index) in getActiveStatuses(c).slice(0, 2)" 
                                        :key="index"
                                        :class="s.class"
                                        class="px-2 py-0.5 rounded text-[10px] font-bold border"
                                    >
                                        {{ s.label }}
                                    </span>
                                    <span 
                                        v-if="getActiveStatuses(c).length > 2"
                                        class="px-1.5 py-0.5 rounded text-[10px] bg-gray-700 text-gray-300 font-medium cursor-help"
                                        :title="getActiveStatuses(c).map(x => x.label).join(', ')"
                                    >
                                        +{{ getActiveStatuses(c).length - 2 }}
                                    </span>
                                </template>
                                <span v-else class="px-2 py-0.5 rounded text-[10px] font-medium bg-gray-700/50 text-gray-500">
                                    Standart
                                </span>
                            </div>
                        </td>

                        <td class="p-4 text-right font-mono text-gray-300 text-sm">
                            {{ c.orderCount || 0 }}
                        </td>

                        <td class="p-4 text-right font-mono text-green-400 font-medium text-sm">
                            {{ formatCurrency(c.totalSpent || 0) }}
                        </td>

                        <td class="p-4 text-gray-300 text-xs">
                            <div class="flex flex-col gap-1">
                                <div v-if="c.phone" class="flex items-center gap-2">
                                    <span class="text-gray-600">📞</span> 
                                    <span class="font-mono">{{ c.phone }}</span>
                                </div>
                                <div v-if="c.email" class="flex items-center gap-2" :title="c.email">
                                    <span class="text-gray-600">📧</span> 
                                    <span class="truncate max-w-[150px]">{{ c.email }}</span>
                                </div>
                            </div>
                        </td>

                        <td class="p-4 text-gray-400 text-xs whitespace-nowrap">
                            <div v-if="c.lastOrderDate" :title="formatDate(c.lastOrderDate)">
                                {{ formatRelativeTime(c.lastOrderDate) }}
                            </div>
                            <div v-else class="text-gray-700 italic">-</div>
                        </td>

                        <td class="p-4 text-right pr-6 whitespace-nowrap">
                            <div class="flex items-center justify-end gap-3">
                                <button 
                                    @click.stop="$emit('row-click', c)" 
                                    class="text-gray-400 hover:text-white transition-colors p-1"
                                    title="Detay"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                                    </svg>
                                </button>

                                <button 
                                    @click.stop="$emit('edit', c)" 
                                    class="text-blue-400 hover:text-blue-300 transition-colors p-1"
                                    title="Düzenle"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                                    </svg>
                                </button>

                                <div class="w-px h-4 bg-gray-700 mx-1"></div>

                                <button 
                                    @click.stop="$emit('delete', c.id)" 
                                    class="text-red-500/50 hover:text-red-400 transition-colors p-1"
                                    title="Sil"
                                >
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                                    </svg>
                                </button>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        
        <div v-if="customers.length === 0 && !isLoading" class="p-16 text-center text-gray-500 flex-1 flex flex-col items-center justify-center">
            <p class="text-lg font-medium mb-1">Kayıt bulunamadı.</p>
            <p class="text-xs">Filtrenizi değiştirmeyi deneyin.</p>
        </div>
    </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { height: 6px; width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: #1E1E2D; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #333; border-radius: 3px; }
</style>