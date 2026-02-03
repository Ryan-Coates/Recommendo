import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '../contexts/AuthContext';
import { Friends } from '../pages/Friends';
import { friendsApi } from '../api/friends';

// Mock the API
jest.mock('../api/friends');

const mockedFriendsApi = friendsApi as jest.Mocked<typeof friendsApi>;

const createTestQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>{component}</AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
};

describe('Friends Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render friends list', async () => {
    mockedFriendsApi.getFriends.mockResolvedValue([
      {
        id: 1,
        userId: 2,
        username: 'testfriend',
        email: 'friend@test.com',
        status: 'Accepted',
        createdAt: new Date().toISOString(),
      },
    ]);

    mockedFriendsApi.getPendingRequests.mockResolvedValue([]);
    mockedFriendsApi.getSentRequests.mockResolvedValue([]);

    renderWithProviders(<Friends />);

    await waitFor(() => {
      expect(screen.getByText('testfriend')).toBeInTheDocument();
    });
  });

  it('should show empty state when no friends', async () => {
    mockedFriendsApi.getFriends.mockResolvedValue([]);
    mockedFriendsApi.getPendingRequests.mockResolvedValue([]);
    mockedFriendsApi.getSentRequests.mockResolvedValue([]);

    renderWithProviders(<Friends />);

    await waitFor(() => {
      expect(screen.getByText('No friends yet!')).toBeInTheDocument();
    });
  });

  it('should display pending requests tab', async () => {
    mockedFriendsApi.getFriends.mockResolvedValue([]);
    mockedFriendsApi.getPendingRequests.mockResolvedValue([
      {
        id: 1,
        userId: 2,
        username: 'requester',
        email: 'requester@test.com',
        status: 'Pending',
        createdAt: new Date().toISOString(),
      },
    ]);
    mockedFriendsApi.getSentRequests.mockResolvedValue([]);

    renderWithProviders(<Friends />);

    await waitFor(() => {
      const pendingTab = screen.getByText(/Pending/);
      expect(pendingTab).toBeInTheDocument();
    });
  });
});
