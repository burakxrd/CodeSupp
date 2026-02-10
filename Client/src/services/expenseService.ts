import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface Expense {
    id: number;
    description?: string;
    amount?: number;
    date?: string;
    categoryId?: number;
    [key: string]: any;
}

export interface ExpenseListResponse {
    items: Expense[];
    total: number;
    page: number;
    pageSize: number;
}

export default {
    getExpenses(
        page: number = 1, 
        pageSize: number = 20, 
        search: string = '', 
        sortBy: string = 'date', 
        sortDir: string = 'desc', 
        startDate: string | null = null, 
        endDate: string | null = null
    ): Promise<ExpenseListResponse> { 
        return apiClient.get(API_ROUTES.FINANCE.EXPENSES, {
            params: { page, pageSize, search, sortBy, sortDir, startDate, endDate }
        }).then(res => res.data); 
    },
    
    createExpense(data: Omit<Expense, 'id'>): Promise<Expense> { 
        return apiClient.post(API_ROUTES.FINANCE.EXPENSES, data).then(res => res.data); 
    },
    
    updateExpense(id: number, data: Partial<Expense>): Promise<Expense> { 
        return apiClient.put(`${API_ROUTES.FINANCE.EXPENSES}/${id}`, data).then(res => res.data); 
    },
    
    deleteExpense(id: number): Promise<void> { 
        return apiClient.delete(`${API_ROUTES.FINANCE.EXPENSES}/${id}`).then(res => res.data); 
    }
};