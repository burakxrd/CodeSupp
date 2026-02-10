import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface Category {
    id: number;
    name: string;
    [key: string]: any;
}

export default {
    getCategories(): Promise<Category[]> { 
        // DİKKAT: Sona .LIST ekledik. Artık string dönüyor.
        return apiClient.get(API_ROUTES.SETTINGS.CATEGORIES.LIST).then(res => res.data); 
    },
    
    createCategory(data: Omit<Category, 'id'>): Promise<Category> { 
        // DİKKAT: Sona .CREATE ekledik.
        return apiClient.post(API_ROUTES.SETTINGS.CATEGORIES.CREATE, data).then(res => res.data); 
    },
    
    updateCategory(id: number, data: Partial<Category>): Promise<Category> { 
        // DİKKAT: .UPDATE bir fonksiyon, (id) parametresi alıyor.
        return apiClient.put(API_ROUTES.SETTINGS.CATEGORIES.UPDATE(id), data).then(res => res.data); 
    },
    
    deleteCategory(id: number): Promise<void> { 
        // DİKKAT: .DELETE de bir fonksiyon.
        return apiClient.delete(API_ROUTES.SETTINGS.CATEGORIES.DELETE(id)).then(res => res.data); 
    }
};