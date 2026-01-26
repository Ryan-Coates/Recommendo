import api from './client';
import type { Friend, InviteLink } from '../types';

export const friendsApi = {
  getFriends: async (): Promise<Friend[]> => {
    const response = await api.get<Friend[]>('/friends');
    return response.data;
  },

  generateInviteLink: async (): Promise<InviteLink> => {
    const response = await api.post<InviteLink>('/friends/invite');
    return response.data;
  },

  acceptInvite: async (token: string): Promise<{ message: string }> => {
    const response = await api.post('/friends/invite/accept', { token });
    return response.data;
  },

  removeFriend: async (friendId: number): Promise<{ message: string }> => {
    const response = await api.delete(`/friends/${friendId}`);
    return response.data;
  },
};
