import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/*
================================================================
PrivateRoute — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- jQuery dashboard-এ manually token check করতাম
- React Router-এ PrivateRoute দিয়ে protected pages handle করি
- user আছে → children (protected page) দেখাও
- user নেই → /login এ redirect করো
USAGE:
  <Route path="/dashboard" element={
    <PrivateRoute><Dashboard /></PrivateRoute>
  } />
================================================================
*/
export default function PrivateRoute({ children }) {
    const { user } = useAuth();
    return user ? children : <Navigate to="/login" replace />;
}
