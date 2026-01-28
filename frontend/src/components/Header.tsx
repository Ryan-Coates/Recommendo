import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Header.css';

export const Header = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  if (!user) {
    return null;
  }

  return (
    <header className="header">
      <div className="header-content">
        <Link to="/" className="logo">
          Recommendo
        </Link>
        <nav className="nav">
          <Link to="/" className="nav-link">Home</Link>
          <Link to="/friends" className="nav-link">Friends</Link>
          {user.isAdmin && (
            <Link to="/admin" className="nav-link admin-link">Admin</Link>
          )}
          <span className="username">{user.username}</span>
          <button onClick={handleLogout} className="logout-btn">
            Logout
          </button>
        </nav>
      </div>
    </header>
  );
};
