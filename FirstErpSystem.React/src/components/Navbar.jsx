import { useAuth } from '../context/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

/*
================================================================
Navbar.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Reusable component — সব page-এ use করা যাবে
- useAuth() থেকে user info নেওয়া
- useNavigate() দিয়ে logout করে /login এ যাওয়া
- Link = React Router এর <a> tag
  page reload হয় না — SPA navigation
================================================================
*/
export default function Navbar() {
    const { user, logout } = useAuth();
    const navigate         = useNavigate();

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    return (
        <nav className="navbar navbar-dark bg-dark px-4 py-2"
             style={{ boxShadow:'0 2px 8px rgba(0,0,0,0.4)', position:'sticky', top:0, zIndex:1000 }}>
            <Link className="navbar-brand fw-bold text-decoration-none" to="/dashboard">
                <i className="bi bi-building-gear me-2"></i>First ERP System
            </Link>
            <div className="d-flex align-items-center gap-3">
                {/* User info */}
                <div className="d-flex align-items-center gap-2">
                    <div style={{ fontSize:28, color:'#adb5bd', lineHeight:1 }}>
                        <i className="bi bi-person-circle"></i>
                    </div>
                    <div>
                        <div className="text-white fw-semibold small">{user?.fullName}</div>
                        <div className="text-white-50" style={{ fontSize:11 }}>{user?.role}</div>
                    </div>
                </div>
                {/* Nav links */}
                <Link className="btn btn-outline-light btn-sm" to="/products">
                    <i className="bi bi-box-seam me-1"></i>Inventory
                </Link>
                <Link className="btn btn-outline-light btn-sm" to="/purchase-orders">
                    <i className="bi bi-cart me-1"></i>Purchase
                </Link>
                <Link className="btn btn-outline-light btn-sm" to="/sales-orders">
                    <i className="bi bi-bag me-1"></i>Sales
                </Link>
                {/* Logout */}
                <button className="btn btn-outline-danger btn-sm" onClick={handleLogout}>
                    <i className="bi bi-box-arrow-right me-1"></i>Logout
                </button>
            </div>
        </nav>
    );
}
