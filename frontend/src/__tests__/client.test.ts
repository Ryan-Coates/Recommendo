import axios from 'axios';
import api from '../api/client';

jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('API Client', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorage.clear();
  });

  it('should include auth token in requests when available', () => {
    const token = 'test-token';
    localStorage.setItem('token', token);

    // The interceptor should add the token
    const config = api.interceptors.request.handlers[0].fulfilled({ headers: {} } as any);
    
    expect(config.headers.Authorization).toBe(`Bearer ${token}`);
  });

  it('should not include auth token when not available', () => {
    const config = api.interceptors.request.handlers[0].fulfilled({ headers: {} } as any);
    
    expect(config.headers.Authorization).toBeUndefined();
  });
});
