import apiClient from './httpClient';
import { API_ROUTES } from '../constants/apiRoutes';

export interface AnalysisResult {
    [key: string]: any;
}

export default {
    analyzeDocument(formData: FormData): Promise<AnalysisResult> {
        return apiClient.post(API_ROUTES.INTEGRATION.ANALYZE, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        }).then(res => res.data);
    }
};