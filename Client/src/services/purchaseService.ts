import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

// Backend'e gönderilecek veri modeli (Payload)
export interface PurchasePayload {
    id?: number | null;
    productId: number;
    quantityInUnits: number;
    productPricePerUnit: number;
    totalKg: number;
    shippingCostPerKg: number;
    description?: string;
    purchaseDate: string;
}

export default {
    getPurchaseHistory(page: number = 1, pageSize: number = 20) { 
        return apiClient.get(API_ROUTES.PURCHASES.BASE, {
            params: { page, pageSize }
        }).then(res => res.data); 
    },
    
    getPurchaseCreateData() { 
        return apiClient.get(API_ROUTES.PURCHASES.CREATE_DATA).then(res => res.data); 
    }, 
    
    createPurchase(data: PurchasePayload) { 
        return apiClient.post(API_ROUTES.PURCHASES.BASE, data).then(res => res.data); 
    },
    
    createBulkPurchase(data: PurchasePayload[]) { 
        return apiClient.post(API_ROUTES.PURCHASES.CREATE_BULK, data).then(res => res.data); 
    },
    
    updatePurchase(id: number, data: PurchasePayload) { 
        return apiClient.put(API_ROUTES.PURCHASES.UPDATE(id), data).then(res => res.data); 
    },
    
    deletePurchase(id: number) { 
        return apiClient.delete(API_ROUTES.PURCHASES.DELETE(id)).then(res => res.data); 
    }
};