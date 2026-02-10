import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface Customer {
    id: number;
    name: string;
    email?: string | null;
    phone?: string | null;
    instagramHandle?: string | null;
    address?: string | null;
    [key: string]: any;
}

export interface CustomerListResponse {
    items: Customer[];
    total: number;
    page: number;
    pageSize: number;
}

export default {
    getCustomers(page: number = 1, pageSize: number = 20, search: string = '', status: string | null = null): Promise<CustomerListResponse> {
        const params: any = { page, pageSize };
        if (search) params.search = search;
        if (status) params.status = status;

        return apiClient.get(API_ROUTES.CUSTOMERS.GET_LIST, { params })
            .then(res => res.data);
    },

    getCustomerById(id: number): Promise<Customer> {
        return apiClient.get(API_ROUTES.CUSTOMERS.UPDATE(id)).then(res => res.data);
    },

    createCustomer(data: Omit<Customer, 'id'>): Promise<Customer> {
        return apiClient.post(API_ROUTES.CUSTOMERS.CREATE, data).then(res => res.data);
    },

    updateCustomer(id: number, data: Partial<Customer>): Promise<Customer> {
        return apiClient.put(API_ROUTES.CUSTOMERS.UPDATE(id), data).then(res => res.data);
    },

    deleteCustomer(id: number): Promise<void> {
        return apiClient.delete(API_ROUTES.CUSTOMERS.DELETE(id)).then(res => res.data);
    }
};