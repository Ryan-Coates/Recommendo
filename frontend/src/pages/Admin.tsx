import { useQuery } from '@tanstack/react-query';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { adminApi } from '../api/admin';
import type { User } from '../types';
import './Admin.css';

export const Admin = () => {
  const { user } = useAuth();

  const { data: users = [], isLoading: usersLoading } = useQuery<User[]>({
    queryKey: ['admin-users'],
    queryFn: adminApi.getUsers,
    enabled: !!user?.isAdmin,
  });

  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: adminApi.getStats,
    enabled: !!user?.isAdmin,
  });

  if (!user?.isAdmin) {
    return <Navigate to="/" replace />;
  }

  return (
    <div className="admin-container">
      <h1>Admin Dashboard</h1>

      <div className="stats-grid">
        {statsLoading ? (
          <div>Loading stats...</div>
        ) : (
          <>
            <div className="stat-card">
              <div className="stat-value">{stats?.totalUsers || 0}</div>
              <div className="stat-label">Total Users</div>
            </div>
            <div className="stat-card">
              <div className="stat-value">{stats?.totalRecommendations || 0}</div>
              <div className="stat-label">Total Recommendations</div>
            </div>
            <div className="stat-card">
              <div className="stat-value">{stats?.totalFriendships || 0}</div>
              <div className="stat-label">Total Friendships</div>
            </div>
          </>
        )}
      </div>

      <div className="users-section">
        <h2>All Users</h2>
        {usersLoading ? (
          <div className="loading">Loading users...</div>
        ) : (
          <div className="users-table-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Created</th>
                  <th>Admin</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td>{u.id}</td>
                    <td>{u.username}</td>
                    <td>{u.email}</td>
                    <td>{new Date(u.createdAt).toLocaleDateString()}</td>
                    <td>{u.isAdmin ? '✓' : ''}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
