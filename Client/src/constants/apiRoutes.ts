/**
 * API Uç Noktaları (Endpoints)
 * src/constants/apiRoutes.ts
 */

export const API_ROUTES = {
    // --- KİMLİK DOĞRULAMA ---
    AUTH: {
        LOGIN: '/auth/login',
        REGISTER: '/auth/register',
        LOGOUT: '/auth/logout',
        ME: '/auth/me',
        REFRESH_TOKEN: '/auth/refresh-token' 
    },

    // --- ENTEGRASYON ---
    INTEGRATION: {
        ANALYZE: '/integration/analyze'
    },

    // --- SİPARİŞ & SATIŞ ---
    SALES: {
        BASE: '/sales',
        GET_LIST: '/sales',
        CREATE: '/sales',
        CREATE_BULK: '/sales/bulk',
        CREATE_DATA: '/sales/createdata', 
        DELETE: (id: number | string) => `/sales/${id}`,
        UPDATE: (id: number | string) => `/sales/${id}`,
        UPDATE_SHIPPING: (id: number | string) => `/sales/${id}/shipping`, 
        GET_DETAILS: (id: number | string) => `/sales/${id}`
    },

    // --- ÜRÜN YÖNETİMİ ---
    PRODUCTS: {
        BASE: '/products',
        GET_LIST: '/products',
        CREATE: '/products',
        UPDATE: (id: number | string) => `/products/${id}`,
        DELETE: (id: number | string) => `/products/${id}`,
        ADJUST_STOCK: (id: number | string) => `/products/${id}/adjust-stock`,
        GENERATE_CODES: '/products/generate-missing-codes',
        UPLOAD_IMAGE: (id: number | string) => `/products/${id}/image`
    },

    // --- SATIN ALMA (PURCHASES) ---
    PURCHASES: {
        BASE: '/product-purchase',
        CREATE_DATA: '/product-purchase/create-data',
        CREATE_BULK: '/product-purchase/bulk',
        UPDATE: (id: number | string) => `/product-purchase/${id}`,
        DELETE: (id: number | string) => `/product-purchase/${id}`
    },

    // --- MÜŞTERİ YÖNETİMİ ---
    CUSTOMERS: {
        BASE: '/customers',
        GET_LIST: '/customers',
        CREATE: '/customers',
        UPDATE: (id: number | string) => `/customers/${id}`,
        DELETE: (id: number | string) => `/customers/${id}`,
        SEARCH: '/customers/search',
        GET_PROFILE: (id: number | string) => `/customers/${id}/profile`
    },

    // --- FİNANS (BURASI GÜNCELLENDİ) ---
    FINANCE: {
        EXPENSES: '/expenses', 
        PAYMENTS: '/payments', 
        TRANSACTIONS: '/finance/transactions',
        STATS: '/finance/stats',              
        ADD_INCOME: '/finance/income',       
        ADD_EXPENSE: '/finance/expense'        
    },

    // --- DASHBOARD ---
    DASHBOARD: {
        BASE: '/dashboard',
        STATS: '/dashboard/stats', 
        FINANCE_SUMMARY: '/dashboard/finance-summary',
        WIDGETS: '/dashboard/widgets',
        CHART_DATA: '/dashboard/chart-data'
    },

    // --- AYARLAR ---
    SETTINGS: {
        PROFILE: '/settings/profile',
        APP_CONFIG: '/settings/config',
        DASHBOARD_CONFIG: '/user/dashboard-settings',
        CATEGORIES: {
            LIST: '/categories',
            CREATE: '/categories',
            UPDATE: (id: number) => `/categories/${id}`,
            DELETE: (id: number) => `/categories/${id}`
        }
    }
};