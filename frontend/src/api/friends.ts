import api from './client';
import type { Friend, InviteLink, SearchUser } from '../types';

export const friendsApi = {
  getFriends: async (): Promise<Friend[]> => {
    const response = await api.get<Friend[]>('/friends');
    return response.data;
  },

  getPendingRequests: async (): Promise<Friend[]> => {
    const response = await api.get<Friend[]>('/friends/pending');
    return response.data;
  },

  getSentRequests: async (): Promise<Friend[]> => {
    const response = await api.get<Friend[]>('/friends/sent');
    return response.data;
  },

  searchUsers: async (query: string): Promise<SearchUser[]> => {
    const response = await api.get<SearchUser[]>('/friends/search', {
      params: { query },
    });
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

  sendFriendRequest: async (targetUserId: number): Promise<{ message: string }> => {
    const response = await api.post('/friends/request', { targetUserId });
    return response.data;
  },

  respondToRequest: async (friendshipId: number, accept: boolean): Promise<{ message: string }> => {
    const response = await api.post('/friends/request/respond', { friendshipId, accept });
    return response.data;
  },

  removeFriend: async (friendId: number): Promise<{ message: string }> => {
    const response = await api.delete(`/friends/${friendId}`);
    return response.data;
  },
};
