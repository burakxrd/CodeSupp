import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface Sale {
    id: number;
    date: string;
    totalAmount: number;
    shippingStatus: string;
    paymentStatus: string;
    customerId?: number;
    customerName?: string;
    items?: any[];
    rowVersion?: string;     
    [key: string]: any;
}

export interface SaleListResponse {
    items: Sale[];
    total: number;
    page: number;
    pageSize: number;
}

export default {
    // Liste Getir
    getSales(
        page: number = 1, 
        pageSize: number = 10, 
        search: string = '', 
        shippingStatus: string | null = null, 
        paymentStatus: string | null = null, 
        startDate: string | null = null, 
        endDate: string | null = null, 
        sortBy: string = 'date', 
        sortDir: string = 'desc',
        customerId: number | null = null
    ): Promise<SaleListResponse> { 
        // DİKKAT: Burada artık ORDERS değil SALES kullanıyoruz
        return apiClient.get(API_ROUTES.SALES.GET_LIST, { 
            params: { 
                pageNumber: page, 
                pageSize, 
                search, 
                shippingStatus, 
                paymentStatus, 
                startDate, 
                endDate, 
                sortBy, 
                sortDir,
                customerId 
            } 
        }).then(res => res.data); 
    },

    // Satış Oluşturma Verisi (Ürünler, Müşteriler vb.)
    getSaleCreateData(): Promise<any> { 
        return apiClient.get(API_ROUTES.SALES.CREATE_DATA).then(res => res.data); 
    },
    
    // Satış Ekle
    createSale(data: any): Promise<Sale> { 
        return apiClient.post(API_ROUTES.SALES.CREATE, data).then(res => res.data); 
    },
    
    // Satış Güncelle
    updateSale(id: number, data: any): Promise<Sale> {
        return apiClient.put(API_ROUTES.SALES.UPDATE(id), data).then(res => res.data);
    },
    
    // Toplu Satış Ekle
    createBulkSale(data: any[]): Promise<any> { 
        return apiClient.post(API_ROUTES.SALES.CREATE_BULK, { items: data }).then(res => res.data); 
    },
    
    // Satış Sil
    deleteSale(id: number): Promise<void> { 
        return apiClient.delete(API_ROUTES.SALES.DELETE(id)).then(res => res.data); 
    },
    
    // Kargo Durumu Güncelle
    updateShippingStatus(id: number, status: number | string): Promise<Sale> {
        return apiClient.patch(
            API_ROUTES.SALES.UPDATE_SHIPPING(id), 
            { status: status } 
        ).then(res => res.data);
    }
};