export interface User {
  id: number;
  email: string;
  username: string;
  createdAt: string;
}

export interface AuthResponse {
  id: number;
  email: string;
  username: string;
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  username: string;
  password: string;
}

export interface Friend {
  id: number;
  userId: number;
  username: string;
  email: string;
  status: string;
  createdAt: string;
}

export interface InviteLink {
  token: string;
  inviteUrl: string;
  expiresAt: string;
}

export interface Recommendation {
  id: number;
  createdByUserId: number;
  createdByUsername: string;
  recommendedToUserId: number;
  recommendedToUsername: string;
  title: string;
  type: string;
  description?: string;
  externalId?: string;
  status: string;
  createdAt: string;
  completedAt?: string;
}

export interface CreateRecommendationRequest {
  title: string;
  type: string;
  description?: string;
  externalId?: string;
  recommendedToUserIds: number[];
}
