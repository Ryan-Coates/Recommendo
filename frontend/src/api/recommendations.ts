import api from './client';
import type { Recommendation, CreateRecommendationRequest } from '../types';

export const recommendationsApi = {
  getRecommendations: async (filters?: { type?: string; friendId?: number }): Promise<Recommendation[]> => {
    const params = new URLSearchParams();
    if (filters?.type) params.append('type', filters.type);
    if (filters?.friendId) params.append('friendId', filters.friendId.toString());
    
    const response = await api.get<Recommendation[]>(`/recommendations?${params}`);
    return response.data;
  },

  getRecommendation: async (id: number): Promise<Recommendation> => {
    const response = await api.get<Recommendation>(`/recommendations/${id}`);
    return response.data;
  },

  createRecommendation: async (data: CreateRecommendationRequest): Promise<Recommendation[]> => {
    const response = await api.post<Recommendation[]>('/recommendations', data);
    return response.data;
  },

  updateStatus: async (id: number, status: string): Promise<{ message: string }> => {
    const response = await api.put(`/recommendations/${id}`, { status });
    return response.data;
  },

  deleteRecommendation: async (id: number): Promise<{ message: string }> => {
    const response = await api.delete(`/recommendations/${id}`);
    return response.data;
  },

  getTypes: async (): Promise<string[]> => {
    const response = await api.get<string[]>('/recommendations/types');
    return response.data;
  },
};
