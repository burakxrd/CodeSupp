// src/utils/formatters.ts

/**
 * Para birimini formatlar (Örn: ₺1.250,50)
 * @param value Formatlanacak sayı
 */
export const formatCurrency = (value: number | null | undefined): string => {
    if (value === null || value === undefined) return '₺0,00';
    
    return new Intl.NumberFormat('tr-TR', { 
        style: 'currency', 
        currency: 'TRY' 
    }).format(value);
};

/**
 * Tarihi formatlar (Örn: 9 Şubat 2026)
 * ✅ DÜZELTME: Backend'den gelen "2026-02-08T21:00:00" formatı için UTC kullan
 * @param date Tarih objesi veya string
 */
export const formatDate = (date: string | Date | number | null | undefined): string => {
    if (!date) return '-';
    
    let dateStr = '';
    if (typeof date === 'string') {
        dateStr = date;
    } else if (date instanceof Date) {
        dateStr = date.toISOString();
    } else {
        dateStr = new Date(date).toISOString();
    }
    
    // YYYY-MM-DD kısmını al
    const datePart = dateStr.split('T')[0];
    const [year, month, day] = datePart.split('-').map(Number);
    
    // UTC olarak oluştur ve UTC olarak formatla (timezone kayması olmasın)
    const utcDate = new Date(Date.UTC(year, month - 1, day));
    
    return utcDate.toLocaleDateString('tr-TR', { 
        day: 'numeric', 
        month: 'long', 
        year: 'numeric',
        timeZone: 'UTC'
    });
};

/**
 * Tarih ve saati formatlar (Örn: 9 Şubat 2026, 14:30)
 * ✅ Loglama ve detaylı kayıtlar için saat bilgisi ile birlikte gösterir
 * @param date Tarih objesi veya string
 */
export const formatDateTime = (date: string | Date | number | null | undefined): string => {
    if (!date) return '-';
    
    let dateStr = '';
    if (typeof date === 'string') {
        dateStr = date;
    } else if (date instanceof Date) {
        dateStr = date.toISOString();
    } else {
        dateStr = new Date(date).toISOString();
    }
    
    // YYYY-MM-DD kısmını al
    const datePart = dateStr.split('T')[0];
    const [year, month, day] = datePart.split('-').map(Number);
    
    // Saat bilgisini al (varsa)
    let hours = 0, minutes = 0;
    if (dateStr.includes('T')) {
        const timePart = dateStr.split('T')[1];
        const timeComponents = timePart.split(':');
        hours = parseInt(timeComponents[0]) || 0;
        minutes = parseInt(timeComponents[1]) || 0;
    }
    
    // UTC olarak oluştur
    const utcDate = new Date(Date.UTC(year, month - 1, day, hours, minutes));
    
    return utcDate.toLocaleString('tr-TR', { 
        day: 'numeric', 
        month: 'long', 
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        timeZone: 'UTC'
    });
};

/**
 * Tarihi "3 gün önce" formatına çevirir
 * ✅ DÜZELTME: UTC kullan
 * @param date Tarih objesi veya string
 */
export const formatRelativeTime = (date: string | Date | number | null | undefined): string => {
    if (!date) return '-';
    
    let dateStr = '';
    if (typeof date === 'string') {
        dateStr = date;
    } else if (date instanceof Date) {
        dateStr = date.toISOString();
    } else {
        dateStr = new Date(date).toISOString();
    }
    
    const datePart = dateStr.split('T')[0];
    const [year, month, day] = datePart.split('-').map(Number);
    
    const past = new Date(Date.UTC(year, month - 1, day));
    const now = new Date();
    const nowDate = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()));
    
    const diffTime = nowDate.getTime() - past.getTime(); 
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24)); 

    if (diffDays < 0) return formatDate(date);
    if (diffDays === 0) return 'Bugün';
    if (diffDays === 1) return 'Dün';
    if (diffDays < 30) return `${diffDays} gün önce`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} ay önce`;
    return `${Math.floor(diffDays / 365)} yıl önce`;
};

/**
 * Instagram kullanıcı adından profil resmi URL'i oluşturur
 * @param handle Kullanıcı adı (@'li veya @'siz)
 */
export const getAvatarUrl = (handle: string | null | undefined): string | null => {
    if (!handle) return null;
    const cleanHandle = handle.replace('@', '').trim();
    if (!cleanHandle) return null;
    
    return `https://unavatar.io/instagram/${cleanHandle}`;
};

// --- FİNANSAL HESAPLAMALAR İÇİN TİP TANIMI ---
interface SaleFinancials {
    totalAmount?: number;
    shippingCost?: number;
    platformCommission?: number;
    taxAmount?: number;
    [key: string]: any;
}

export const calculateNetProfit = (sale: SaleFinancials | null | undefined): number => {
    if (!sale) return 0;
    const revenue = sale.totalAmount || 0;
    const expenses = (sale.shippingCost || 0) + (sale.platformCommission || 0) + (sale.taxAmount || 0);
    return revenue - expenses;
};

export const calculateTotalDeductions = (sale: SaleFinancials | null | undefined): number => {
    if (!sale) return 0;
    return (sale.shippingCost || 0) + (sale.platformCommission || 0) + (sale.taxAmount || 0);
};