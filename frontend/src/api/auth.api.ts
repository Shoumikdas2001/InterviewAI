import { apiClient, setTokens, clearTokens } from './client';
import type { ApiResponse, AuthResponse, LoginRequest, RegisterRequest, User } from '../types';

export const authApi = {
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const res = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/register', data);
    const { accessToken, refreshToken, user } = res.data.data;
    setTokens(accessToken, refreshToken);
    localStorage.setItem('user', JSON.stringify(user));
    return res.data.data;
  },

  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const res = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/login', data);
    const { accessToken, refreshToken, user } = res.data.data;
    setTokens(accessToken, refreshToken);
    localStorage.setItem('user', JSON.stringify(user));
    return res.data.data;
  },

  logout: async (): Promise<void> => {
    const refreshToken = localStorage.getItem('refreshToken') || '';
    try {
      await apiClient.post('/api/auth/logout', { refreshToken });
    } finally {
      clearTokens();
    }
  },

  getProfile: async (): Promise<User> => {
    const res = await apiClient.get<ApiResponse<User>>('/api/auth/profile');
    return res.data.data;
  },

  forgotPassword: async (email: string): Promise<void> => {
    await apiClient.post('/api/auth/forgot-password', { email });
  },

  resetPassword: async (token: string, email: string, newPassword: string): Promise<void> => {
    await apiClient.post('/api/auth/reset-password', { token, email, newPassword });
  },
};
