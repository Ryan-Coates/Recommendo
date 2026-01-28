import api from './client';
import type { User } from '../types';

interface AdminStats {
  totalUsers: number;
  totalRecommendations: number;
  totalFriendships: number;
}

export const adminApi = {
  getUsers: async (): Promise<User[]> => {
    const response = await api.get<User[]>('/admin/users');
    return response.data;
  },

  getStats: async (): Promise<AdminStats> => {
    const response = await api.get<AdminStats>('/admin/stats');
    return response.data;
  },
};
