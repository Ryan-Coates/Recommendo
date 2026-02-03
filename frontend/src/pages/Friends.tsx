import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { friendsApi } from '../api/friends';
import type { Friend, InviteLink, SearchUser } from '../types';
import './Friends.css';

type TabType = 'friends' | 'pending' | 'sent';

export const Friends = () => {
  const { inviteToken } = useParams();
  const [activeTab, setActiveTab] = useState<TabType>('friends');
  const [showInviteLink, setShowInviteLink] = useState(false);
  const [inviteLink, setInviteLink] = useState<InviteLink | null>(null);
  const [copySuccess, setCopySuccess] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchUser[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const queryClient = useQueryClient();

  const { data: friends = [], isLoading } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: friendsApi.getFriends,
  });

  const { data: pendingRequests = [] } = useQuery<Friend[]>({
    queryKey: ['pendingRequests'],
    queryFn: friendsApi.getPendingRequests,
  });

  const { data: sentRequests = [] } = useQuery<Friend[]>({
    queryKey: ['sentRequests'],
    queryFn: friendsApi.getSentRequests,
  });

  const generateInviteMutation = useMutation({
    mutationFn: friendsApi.generateInviteLink,
    onSuccess: (data) => {
      setInviteLink(data);
      setShowInviteLink(true);
    },
  });

  const acceptInviteMutation = useMutation({
    mutationFn: friendsApi.acceptInvite,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sentRequests'] });
      alert('Friend request sent successfully!');
    },
    onError: () => {
      alert('Failed to accept invite. The link may be invalid or expired.');
    },
  });

  const sendRequestMutation = useMutation({
    mutationFn: friendsApi.sendFriendRequest,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sentRequests'] });
      setSearchQuery('');
      setSearchResults([]);
      alert('Friend request sent!');
    },
    onError: () => {
      alert('Failed to send friend request.');
    },
  });

  const respondToRequestMutation = useMutation({
    mutationFn: ({ friendshipId, accept }: { friendshipId: number; accept: boolean }) =>
      friendsApi.respondToRequest(friendshipId, accept),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['pendingRequests'] });
      queryClient.invalidateQueries({ queryKey: ['friends'] });
      alert(variables.accept ? 'Friend request accepted!' : 'Friend request rejected.');
    },
  });

  const removeFriendMutation = useMutation({
    mutationFn: friendsApi.removeFriend,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['friends'] });
    },
  });

  // Auto-accept invite if token in URL
  useEffect(() => {
    if (inviteToken) {
      acceptInviteMutation.mutate(inviteToken);
    }
  }, [inviteToken]);

  // Search users with debounce
  useEffect(() => {
    if (searchQuery.length < 2) {
      setSearchResults([]);
      return;
    }

    setIsSearching(true);
    const timer = setTimeout(async () => {
      try {
        const results = await friendsApi.searchUsers(searchQuery);
        setSearchResults(results);
      } catch (error) {
        console.error('Search failed:', error);
      } finally {
        setIsSearching(false);
      }
    }, 500);

    return () => clearTimeout(timer);
  }, [searchQuery]);

  const handleGenerateInvite = () => {
    generateInviteMutation.mutate();
  };

  const handleCopyLink = () => {
    if (inviteLink) {
      const fullUrl = `${window.location.origin}/invite/${inviteLink.token}`;
      navigator.clipboard.writeText(fullUrl);
      setCopySuccess(true);
      setTimeout(() => setCopySuccess(false), 2000);
    }
  };

  const handleRemoveFriend = (friendId: number, username: string) => {
    if (confirm(`Remove ${username} from your friends?`)) {
      removeFriendMutation.mutate(friendId);
    }
  };

  const handleSendRequest = (userId: number) => {
    sendRequestMutation.mutate(userId);
  };

  const handleRespondToRequest = (friendshipId: number, accept: boolean) => {
    respondToRequestMutation.mutate({ friendshipId, accept });
  };

  const renderFriendsList = () => {
    if (isLoading) {
      return <div className="loading">Loading friends...</div>;
    }

    if (friends.length === 0) {
      return (
        <div className="empty-state">
          <p>No friends yet!</p>
          <p className="empty-subtitle">Search for users or generate an invite link to add friends.</p>
        </div>
      );
    }

    return (
      <div className="friends-grid">
        {friends.map((friend) => (
          <div key={friend.id} className="friend-card">
            <div className="friend-info">
              <div className="friend-avatar">
                {friend.username.charAt(0).toUpperCase()}
              </div>
              <div className="friend-details">
                <h3 className="friend-name">{friend.username}</h3>
                <p className="friend-email">{friend.email}</p>
                <p className="friend-since">
                  Friends since {new Date(friend.createdAt).toLocaleDateString()}
                </p>
              </div>
            </div>
            <button
              onClick={() => handleRemoveFriend(friend.userId, friend.username)}
              className="remove-btn"
            >
              Remove
            </button>
          </div>
        ))}
      </div>
    );
  };

  const renderPendingRequests = () => {
    if (pendingRequests.length === 0) {
      return (
        <div className="empty-state">
          <p>No pending friend requests</p>
        </div>
      );
    }

    return (
      <div className="friends-grid">
        {pendingRequests.map((request) => (
          <div key={request.id} className="friend-card">
            <div className="friend-info">
              <div className="friend-avatar">
                {request.username.charAt(0).toUpperCase()}
              </div>
              <div className="friend-details">
                <h3 className="friend-name">{request.username}</h3>
                <p className="friend-email">{request.email}</p>
                <p className="friend-since">
                  Requested {new Date(request.createdAt).toLocaleDateString()}
                </p>
              </div>
            </div>
            <div className="request-actions">
              <button
                onClick={() => handleRespondToRequest(request.id, true)}
                className="accept-btn"
              >
                Accept
              </button>
              <button
                onClick={() => handleRespondToRequest(request.id, false)}
                className="reject-btn"
              >
                Reject
              </button>
            </div>
          </div>
        ))}
      </div>
    );
  };

  const renderSentRequests = () => {
    if (sentRequests.length === 0) {
      return (
        <div className="empty-state">
          <p>No sent friend requests</p>
        </div>
      );
    }

    return (
      <div className="friends-grid">
        {sentRequests.map((request) => (
          <div key={request.id} className="friend-card">
            <div className="friend-info">
              <div className="friend-avatar">
                {request.username.charAt(0).toUpperCase()}
              </div>
              <div className="friend-details">
                <h3 className="friend-name">{request.username}</h3>
                <p className="friend-email">{request.email}</p>
                <p className="friend-since">
                  Sent {new Date(request.createdAt).toLocaleDateString()}
                </p>
              </div>
            </div>
            <span className="pending-badge">Pending</span>
          </div>
        ))}
      </div>
    );
  };

  return (
    <div className="friends-container">
      <div className="friends-header">
        <h1>Friends</h1>
        <button onClick={handleGenerateInvite} className="invite-btn">
          + Generate Invite Link
        </button>
      </div>

      {showInviteLink && inviteLink && (
        <div className="invite-box">
          <h3>Share this link with your friend</h3>
          <div className="invite-link-container">
            <input
              type="text"
              readOnly
              value={`${window.location.origin}/invite/${inviteLink.token}`}
              className="invite-link-input"
            />
            <button onClick={handleCopyLink} className="copy-btn">
              {copySuccess ? '✓ Copied!' : 'Copy'}
            </button>
          </div>
          <p className="invite-expiry">
            Expires: {new Date(inviteLink.expiresAt).toLocaleString()}
          </p>
          <button onClick={() => setShowInviteLink(false)} className="close-invite-btn">
            Close
          </button>
        </div>
      )}

      <div className="search-section">
        <input
          type="text"
          placeholder="Search for users..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="search-input"
        />
        {isSearching && <div className="search-loading">Searching...</div>}
        {searchResults.length > 0 && (
          <div className="search-results">
            {searchResults.map((user) => (
              <div key={user.id} className="search-result-item">
                <div className="friend-info">
                  <div className="friend-avatar">
                    {user.username.charAt(0).toUpperCase()}
                  </div>
                  <div className="friend-details">
                    <h4 className="friend-name">{user.username}</h4>
                    <p className="friend-email">{user.email}</p>
                  </div>
                </div>
                {user.friendshipStatus === 'None' && (
                  <button
                    onClick={() => handleSendRequest(user.id)}
                    className="add-friend-btn"
                  >
                    Add Friend
                  </button>
                )}
                {user.friendshipStatus === 'Friends' && (
                  <span className="status-badge friends">Friends</span>
                )}
                {user.friendshipStatus === 'RequestSent' && (
                  <span className="status-badge pending">Request Sent</span>
                )}
                {user.friendshipStatus === 'RequestReceived' && (
                  <span className="status-badge pending">Request Received</span>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="tabs">
        <button
          className={`tab ${activeTab === 'friends' ? 'active' : ''}`}
          onClick={() => setActiveTab('friends')}
        >
          Friends ({friends.length})
        </button>
        <button
          className={`tab ${activeTab === 'pending' ? 'active' : ''}`}
          onClick={() => setActiveTab('pending')}
        >
          Pending ({pendingRequests.length})
        </button>
        <button
          className={`tab ${activeTab === 'sent' ? 'active' : ''}`}
          onClick={() => setActiveTab('sent')}
        >
          Sent ({sentRequests.length})
        </button>
      </div>

      <div className="tab-content">
        {activeTab === 'friends' && renderFriendsList()}
        {activeTab === 'pending' && renderPendingRequests()}
        {activeTab === 'sent' && renderSentRequests()}
      </div>
    </div>
  );
};
