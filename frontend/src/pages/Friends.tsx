import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams } from 'react-router-dom';
import { friendsApi } from '../api/friends';
import type { Friend, InviteLink } from '../types';
import './Friends.css';

export const Friends = () => {
  const { inviteToken } = useParams();
  const [showInviteLink, setShowInviteLink] = useState(false);
  const [inviteLink, setInviteLink] = useState<InviteLink | null>(null);
  const [copySuccess, setCopySuccess] = useState(false);
  const queryClient = useQueryClient();

  const { data: friends = [], isLoading } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: friendsApi.getFriends,
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
      queryClient.invalidateQueries({ queryKey: ['friends'] });
      alert('Friend added successfully!');
    },
    onError: () => {
      alert('Failed to accept invite. The link may be invalid or expired.');
    },
  });

  const removeFriendMutation = useMutation({
    mutationFn: friendsApi.removeFriend,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['friends'] });
    },
  });

  // Auto-accept invite if token in URL
  useState(() => {
    if (inviteToken) {
      acceptInviteMutation.mutate(inviteToken);
    }
  });

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

      {isLoading ? (
        <div className="loading">Loading friends...</div>
      ) : friends.length === 0 ? (
        <div className="empty-state">
          <p>No friends yet!</p>
          <p className="empty-subtitle">Generate an invite link to add friends.</p>
        </div>
      ) : (
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
      )}
    </div>
  );
};
