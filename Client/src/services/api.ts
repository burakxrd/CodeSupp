// src/services/api.ts

import apiClient from './httpClient';

import dashboardService from './dashboardService';
import settingsService from './settingsService';
import categoryService from './categoryService';
import productService from './productService';
import customerService from './customerService';
import saleService from './saleService';
import purchaseService from './purchaseService';
import expenseService from './expenseService';
import paymentService from './paymentService';

import type { AxiosRequestConfig, AxiosResponse } from 'axios';

const api = {
    get: <T = any>(url: string, config?: AxiosRequestConfig): Promise<T> => 
        apiClient.get(url, config),

    post: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> => 
        apiClient.post(url, data, config),

    put: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> => 
        apiClient.put(url, data, config),

    delete: <T = any>(url: string, config?: AxiosRequestConfig): Promise<T> => 
        apiClient.delete(url, config),

    patch: <T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> => 
        apiClient.patch(url, data, config),
    
    instance: apiClient, 

    ...dashboardService,
    ...settingsService,
    ...categoryService,
    ...productService,
    ...customerService,
    ...saleService,
    ...purchaseService,
    ...expenseService,
    ...paymentService
};

export default api;