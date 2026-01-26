import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { recommendationsApi } from '../api/recommendations';
import { friendsApi } from '../api/friends';
import type { Recommendation, Friend } from '../types';
import { AddRecommendationModal } from '../components/AddRecommendationModal';
import './Home.css';

export const Home = () => {
  const [selectedTab, setSelectedTab] = useState('All');
  const [selectedFriend, setSelectedFriend] = useState<number | null>(null);
  const [showAddModal, setShowAddModal] = useState(false);
  const queryClient = useQueryClient();

  const { data: recommendations = [], isLoading } = useQuery({
    queryKey: ['recommendations', selectedTab, selectedFriend],
    queryFn: () => recommendationsApi.getRecommendations({
      type: selectedTab === 'All' ? undefined : selectedTab,
      friendId: selectedFriend || undefined,
    }),
  });

  const { data: friends = [] } = useQuery<Friend[]>({
    queryKey: ['friends'],
    queryFn: friendsApi.getFriends,
  });

  const { data: types = [] } = useQuery<string[]>({
    queryKey: ['recommendation-types'],
    queryFn: recommendationsApi.getTypes,
  });

  const updateStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: number; status: string }) =>
      recommendationsApi.updateStatus(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recommendations'] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: recommendationsApi.deleteRecommendation,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recommendations'] });
    },
  });

  const handleStatusChange = (id: number, status: string) => {
    updateStatusMutation.mutate({ id, status });
  };

  const handleDelete = (id: number) => {
    if (confirm('Are you sure you want to delete this recommendation?')) {
      deleteMutation.mutate(id);
    }
  };

  const tabs = ['All', ...(types || [])];

  return (
    <div className="home-container">
      <div className="home-header">
        <h1>Your Recommendations</h1>
        <button onClick={() => setShowAddModal(true)} className="add-btn">
          + Add Recommendation
        </button>
      </div>

      <div className="filters">
        <div className="tabs">
          {tabs.map((tab) => (
            <button
              key={tab}
              className={`tab ${selectedTab === tab ? 'active' : ''}`}
              onClick={() => setSelectedTab(tab)}
            >
              {tab}
            </button>
          ))}
        </div>

        <select
          className="friend-filter"
          value={selectedFriend || ''}
          onChange={(e) => setSelectedFriend(e.target.value ? Number(e.target.value) : null)}
        >
          <option value="">All Friends</option>
          {friends.map((friend) => (
            <option key={friend.userId} value={friend.userId}>
              {friend.username}
            </option>
          ))}
        </select>
      </div>

      {isLoading ? (
        <div className="loading">Loading recommendations...</div>
      ) : recommendations.length === 0 ? (
        <div className="empty-state">
          <p>No recommendations yet!</p>
          <p className="empty-subtitle">Add some recommendations or invite friends to get started.</p>
        </div>
      ) : (
        <div className="recommendations-grid">
          {recommendations.map((rec: Recommendation) => (
            <div key={rec.id} className={`recommendation-card status-${rec.status.toLowerCase()}`}>
              <div className="rec-header">
                <span className="rec-type">{rec.type}</span>
                <span className={`rec-status status-${rec.status.toLowerCase()}`}>
                  {rec.status}
                </span>
              </div>

              <h3 className="rec-title">{rec.title}</h3>

              {rec.description && (
                <p className="rec-description">{rec.description}</p>
              )}

              <div className="rec-footer">
                <span className="rec-from">From: {rec.createdByUsername}</span>
                <span className="rec-date">
                  {new Date(rec.createdAt).toLocaleDateString()}
                </span>
              </div>

              <div className="rec-actions">
                {rec.status === 'Unseen' && (
                  <>
                    <button
                      onClick={() => handleStatusChange(rec.id, 'InProgress')}
                      className="action-btn in-progress"
                    >
                      In Progress
                    </button>
                    <button
                      onClick={() => handleStatusChange(rec.id, 'Watched')}
                      className="action-btn watched"
                    >
                      Mark Watched
                    </button>
                  </>
                )}
                {rec.status === 'InProgress' && (
                  <button
                    onClick={() => handleStatusChange(rec.id, 'Watched')}
                    className="action-btn watched"
                  >
                    Mark Watched
                  </button>
                )}
                <button
                  onClick={() => handleDelete(rec.id)}
                  className="action-btn delete"
                >
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {showAddModal && (
        <AddRecommendationModal
          friends={friends}
          types={types}
          onClose={() => setShowAddModal(false)}
        />
      )}
    </div>
  );
};
