import { defineStore } from 'pinia';
import { ref } from 'vue';
import api from '../services/api'; 

export const useSettingsStore = defineStore('settings', () => {
    // --- STATE ---
    const showShippingColumn = ref<boolean>(false);
    const enableVAT = ref<boolean>(true);
    const defaultVAT = ref<number>(20);
    const isLoading = ref<boolean>(false); 

    // --- ACTIONS ---

    async function fetchSettings(): Promise<void> {
        isLoading.value = true;
        try {
            const response = await api.get('settings'); 
            
            if (response && response.data) {
                const data = response.data;
                showShippingColumn.value = Boolean(data.showShippingColumn);
                enableVAT.value = Boolean(data.enableVAT);
                defaultVAT.value = Number(data.defaultVAT);
            }
        } catch (error) {
            console.error("Ayarlar çekilemedi:", error);
        } finally {
            isLoading.value = false;
        }
    }

    async function saveSettings(): Promise<void> {
        try {
            const payload = {
                showShippingColumn: showShippingColumn.value,
                enableVAT: enableVAT.value,
                defaultVAT: defaultVAT.value
            };
            await api.post('settings', payload); 
        } catch (error) {
            console.error("Ayarlar kaydedilemedi:", error);
        }
    }

    // --- TOGGLES (Değiştir ve Kaydet) ---

    function toggleShippingColumn(): void {
        showShippingColumn.value = !showShippingColumn.value;
        saveSettings(); // Anında DB'ye yaz
    }

    function toggleVAT(): void {
        enableVAT.value = !enableVAT.value;
        saveSettings(); // Anında DB'ye yaz
    }
    
    // Input'tan çıkınca (blur/change) çağrılacak metod
    function updateDefaultVAT(newValue: number): void {
        defaultVAT.value = newValue;
        saveSettings(); // DB'ye yaz
    }

    return { 
        showShippingColumn, 
        enableVAT,
        defaultVAT,
        isLoading,
        fetchSettings,
        saveSettings,
        toggleShippingColumn,
        toggleVAT,
        updateDefaultVAT
    };
});