import { apiClient } from './client';
import type {
  ApiResponse, CreateInterviewRequest, InterviewSession,
  Question, Answer, SubmitAnswerRequest, SkillGapAnalysis, StudyPlan
} from '../types';

interface InterviewWithQuestions {
  session: InterviewSession;
  questions: Question[];
}

export const interviewApi = {
  create: async (data: CreateInterviewRequest): Promise<InterviewWithQuestions> => {
    const res = await apiClient.post<ApiResponse<InterviewWithQuestions>>('/api/interview/create', data);
    return res.data.data;
  },

  getHistory: async (page = 1, pageSize = 10): Promise<InterviewSession[]> => {
    const res = await apiClient.get<ApiResponse<InterviewSession[]>>(
      `/api/interview/history?page=${page}&pageSize=${pageSize}`
    );
    return res.data.data;
  },

  getSession: async (id: string): Promise<InterviewSession> => {
    const res = await apiClient.get<ApiResponse<InterviewSession>>(`/api/interview/${id}`);
    return res.data.data;
  },

  start: async (id: string): Promise<void> => {
    await apiClient.post(`/api/interview/${id}/start`);
  },

  complete: async (id: string): Promise<void> => {
    await apiClient.post(`/api/interview/${id}/complete`);
  },

  getQuestions: async (sessionId: string): Promise<Question[]> => {
    const res = await apiClient.get<ApiResponse<Question[]>>(`/api/interview/${sessionId}/questions`);
    return res.data.data;
  },

  generateFollowUp: async (sessionId: string, questionId: string, answerId: string): Promise<Question[]> => {
    const res = await apiClient.post<ApiResponse<Question[]>>('/api/interview/questions/followup', {
      sessionId, questionId, answerId,
    });
    return res.data.data;
  },

  submitAnswer: async (data: SubmitAnswerRequest): Promise<Answer> => {
    const res = await apiClient.post<ApiResponse<Answer>>('/api/interview/answers/submit', data);
    return res.data.data;
  },

  getAnswers: async (sessionId: string): Promise<Answer[]> => {
    const res = await apiClient.get<ApiResponse<Answer[]>>(`/api/interview/${sessionId}/answers`);
    return res.data.data;
  },

  generateSkillGap: async (sessionId: string): Promise<SkillGapAnalysis> => {
    const res = await apiClient.post<ApiResponse<SkillGapAnalysis>>(`/api/interview/${sessionId}/skill-gap`);
    return res.data.data;
  },

  generateRoadmap: async (jobRole: string, weakAreas: string[], sessionId?: string): Promise<StudyPlan> => {
    const res = await apiClient.post<ApiResponse<StudyPlan>>('/api/interview/roadmap/generate', {
      jobRole, weakAreas, sessionId,
    });
    return res.data.data;
  },

  getLatestRoadmap: async (): Promise<StudyPlan | null> => {
    try {
      const res = await apiClient.get<ApiResponse<StudyPlan>>('/api/interview/roadmap/latest');
      return res.data.data;
    } catch {
      return null;
    }
  },
};

export const dashboardApi = {
  getAnalytics: async () => {
    const res = await apiClient.get('/api/dashboard/analytics');
    return res.data.data;
  },

  getAdminDashboard: async () => {
    const res = await apiClient.get('/api/dashboard/admin');
    return res.data.data;
  },
};

export const reportsApi = {
  generate: async (sessionId: string): Promise<Blob> => {
    const res = await apiClient.post(`/api/reports/${sessionId}/generate`, null, { responseType: 'blob' });
    return res.data;
  },

  get: async (reportId: string): Promise<Blob> => {
    const res = await apiClient.get(`/api/reports/${reportId}`, { responseType: 'blob' });
    return res.data;
  },
};

export const adminApi = {
  getUsers: async (page = 1, pageSize = 20) => {
    const res = await apiClient.get(`/api/admin/users?page=${page}&pageSize=${pageSize}`);
    return res.data.data;
  },

  toggleUserStatus: async (id: string) => {
    const res = await apiClient.patch(`/api/admin/users/${id}/toggle-status`);
    return res.data;
  },
};
