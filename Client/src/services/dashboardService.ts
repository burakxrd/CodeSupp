import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface DashboardData {
    [key: string]: any;
}

export interface FinanceSummary {
    [key: string]: any;
}

export default {
    getDashboardData(startDate: string, endDate: string): Promise<DashboardData> { 
        return apiClient.get(API_ROUTES.DASHBOARD.BASE, { 
            params: { startDate, endDate } 
        }).then(res => res.data); 
    },

    getFinanceSummary(startDate: string, endDate: string): Promise<FinanceSummary> {
        return apiClient.get(API_ROUTES.DASHBOARD.FINANCE_SUMMARY, { 
            params: { startDate, endDate }
        }).then(res => res.data);
    }
};