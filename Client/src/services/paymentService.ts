import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface Payment {
    id: number;
    amount?: number;
    date?: string;
    description?: string;
    type?: string;
    [key: string]: any;
}

export interface PaymentListResponse {
    items: Payment[];
    total: number;
    page: number;
    pageSize: number;
}

export default {
    getPayments(
        page: number = 1, 
        pageSize: number = 20, 
        search: string = '', 
        sortBy: string = 'date', 
        sortDir: string = 'desc', 
        startDate: string | null = null, 
        endDate: string | null = null
    ): Promise<PaymentListResponse> { 
        return apiClient.get(API_ROUTES.FINANCE.PAYMENTS, {
            params: { page, pageSize, search, sortBy, sortDir, startDate, endDate }
        }).then(res => res.data); 
    },
    
    createPayment(data: Omit<Payment, 'id'>): Promise<Payment> { 
        return apiClient.post(API_ROUTES.FINANCE.PAYMENTS, data).then(res => res.data); 
    },
    
    updatePayment(id: number, data: Partial<Payment>): Promise<Payment> { 
        return apiClient.put(`${API_ROUTES.FINANCE.PAYMENTS}/${id}`, data).then(res => res.data); 
    },
    
    deletePayment(id: number): Promise<void> { 
        return apiClient.delete(`${API_ROUTES.FINANCE.PAYMENTS}/${id}`).then(res => res.data); 
    }
};