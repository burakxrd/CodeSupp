import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface Product {
    id: number;
    name: string;
    price: number;
    stock: number;
    sku?: string;
    barcode?: string;
    categoryId?: number;
    description?: string;
    imageUrl?: string | null;
    rowVersion?: string; 
    [key: string]: any;
}

export interface ProductListResponse {
    items: Product[];
    total: number;
    page: number;
    pageSize: number;
}

export default {
    getProducts(
        page: number = 1, 
        pageSize: number = 20, 
        search: string = '', 
        categoryId: number | null = null, 
        sort: string = 'newest'
    ): Promise<ProductListResponse> { 
        return apiClient.get(API_ROUTES.PRODUCTS.GET_LIST, { 
            params: { page, pageSize, search, categoryId, sort } 
        }).then(res => res.data); 
    },
    
    getProductById(id: number): Promise<Product> { 
        return apiClient.get(API_ROUTES.PRODUCTS.UPDATE(id)).then(res => res.data); 
    },
    
    adjustStock(id: number, data: any): Promise<Product> { 
        return apiClient.post(API_ROUTES.PRODUCTS.ADJUST_STOCK(id), data).then(res => res.data); 
    },
    
    generateMissingCodes(): Promise<any> { 
        return apiClient.post(API_ROUTES.PRODUCTS.GENERATE_CODES).then(res => res.data); 
    },
    
    createProduct(data: FormData): Promise<Product> { 
        return apiClient.post(API_ROUTES.PRODUCTS.CREATE, data, {
            headers: { 'Content-Type': 'multipart/form-data' }
        }).then(res => res.data); 
    },
    
    updateProduct(id: number, data: FormData): Promise<Product> { 
        return apiClient.put(API_ROUTES.PRODUCTS.UPDATE(id), data, {
            headers: { 'Content-Type': 'multipart/form-data' }
        }).then(res => res.data); 
    },
    
    deleteProduct(id: number): Promise<void> { 
        return apiClient.delete(API_ROUTES.PRODUCTS.DELETE(id)).then(res => res.data); 
    }
};