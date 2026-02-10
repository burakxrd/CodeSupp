import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface DashboardSettings {
    [key: string]: any; 
}

export default {
    getDashboardSettings() { 
        return apiClient.get(API_ROUTES.SETTINGS.DASHBOARD_CONFIG).then(res => res.data); 
    },
    
    saveDashboardSettings(settings: DashboardSettings) { 
        return apiClient.post(API_ROUTES.SETTINGS.DASHBOARD_CONFIG, settings).then(res => res.data); 
    }
};