import { apiClient } from './client';
import type { ApiResponse, Resume, ResumeUploadResponse, AtsAnalysis } from '../types';

export const resumeApi = {
  upload: async (file: File): Promise<ResumeUploadResponse> => {
    const formData = new FormData();
    formData.append('file', file);
    const res = await apiClient.post<ApiResponse<ResumeUploadResponse>>('/api/resume/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return res.data.data;
  },

  getAll: async (): Promise<Resume[]> => {
    const res = await apiClient.get<ApiResponse<Resume[]>>('/api/resume');
    return res.data.data;
  },

  getById: async (id: string): Promise<Resume> => {
    const res = await apiClient.get<ApiResponse<Resume>>(`/api/resume/${id}`);
    return res.data.data;
  },

  analyze: async (id: string): Promise<AtsAnalysis> => {
    const res = await apiClient.post<ApiResponse<AtsAnalysis>>(`/api/resume/${id}/analyze`);
    return res.data.data;
  },

  download: async (id: string): Promise<Blob> => {
    const res = await apiClient.get(`/api/resume/${id}/download`, { responseType: 'blob' });
    return res.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/api/resume/${id}`);
  },
};
