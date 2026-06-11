import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import PrivateRoute from './components/PrivateRoute';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Products from './pages/Products';
import PurchaseOrders from './pages/PurchaseOrders';
import SalesOrders from './pages/SalesOrders';

/*
================================================================
App.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- BrowserRouter = URL routing enable করে
  jQuery-এ: manually page switch করতাম (show/hide div)
  React-এ: Routes দিয়ে URL-based page switching
- AuthProvider = সব page-এ user info available করে
  wrap করলেই সব child component context access পায়
- PrivateRoute = protected pages
  user না থাকলে /login এ redirect
- Route = কোন URL-এ কোন component দেখাবে
  path="/dashboard" element={<Dashboard />}
  মানে URL-এ /dashboard হলে Dashboard component render হবে
- Navigate = programmatic redirect
  "/" তে গেলে automatically "/dashboard" এ যাবে
================================================================
*/

export default function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    {/* Public route */}
                    <Route path="/login" element={<Login />} />

                    {/* Protected routes */}
                    <Route path="/dashboard" element={
                        <PrivateRoute><Dashboard /></PrivateRoute>
                    } />
                    <Route path="/products" element={
                        <PrivateRoute><Products /></PrivateRoute>
                    } />
                    <Route path="/purchase-orders" element={
                        <PrivateRoute><PurchaseOrders /></PrivateRoute>
                    } />
                    <Route path="/sales-orders" element={
                        <PrivateRoute><SalesOrders /></PrivateRoute>
                    } />

                    {/* Default redirect */}
                    <Route path="*" element={<Navigate to="/dashboard" replace />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}
