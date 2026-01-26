import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { recommendationsApi } from '../api/recommendations';
import type { Friend, CreateRecommendationRequest } from '../types';
import './AddRecommendationModal.css';

interface Props {
  friends: Friend[];
  types: string[];
  onClose: () => void;
}

export const AddRecommendationModal = ({ friends, types, onClose }: Props) => {
  const [title, setTitle] = useState('');
  const [type, setType] = useState('Movie');
  const [description, setDescription] = useState('');
  const [selectedFriends, setSelectedFriends] = useState<number[]>([]);
  const [error, setError] = useState('');
  const queryClient = useQueryClient();

  const createMutation = useMutation({
    mutationFn: (data: CreateRecommendationRequest) =>
      recommendationsApi.createRecommendation(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recommendations'] });
      onClose();
    },
    onError: (err: any) => {
      setError(err.response?.data?.message || 'Failed to create recommendation');
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!title.trim()) {
      setError('Title is required');
      return;
    }

    if (selectedFriends.length === 0) {
      setError('Please select at least one friend');
      return;
    }

    createMutation.mutate({
      title: title.trim(),
      type,
      description: description.trim() || undefined,
      recommendedToUserIds: selectedFriends,
    });
  };

  const toggleFriend = (friendId: number) => {
    setSelectedFriends((prev) =>
      prev.includes(friendId)
        ? prev.filter((id) => id !== friendId)
        : [...prev, friendId]
    );
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Add Recommendation</h2>
          <button onClick={onClose} className="close-btn">&times;</button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          {error && <div className="error-message">{error}</div>}

          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g., The Shawshank Redemption"
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="type">Type *</label>
            <select
              id="type"
              value={type}
              onChange={(e) => setType(e.target.value)}
              required
            >
              {types.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Why should they check this out?"
              rows={3}
            />
          </div>

          <div className="form-group">
            <label>Recommend to *</label>
            {friends.length === 0 ? (
              <p className="no-friends">No friends yet. Add some friends first!</p>
            ) : (
              <div className="friends-list">
                {friends.map((friend) => (
                  <label key={friend.userId} className="friend-checkbox">
                    <input
                      type="checkbox"
                      checked={selectedFriends.includes(friend.userId)}
                      onChange={() => toggleFriend(friend.userId)}
                    />
                    <span>{friend.username}</span>
                  </label>
                ))}
              </div>
            )}
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="cancel-btn">
              Cancel
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending || friends.length === 0}
              className="submit-btn"
            >
              {createMutation.isPending ? 'Adding...' : 'Add Recommendation'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
