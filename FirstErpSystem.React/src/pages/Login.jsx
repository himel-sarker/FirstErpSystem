import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';

/*
================================================================
Login.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- jQuery dashboard-এ: $('#loginBtn').on('click', function(){})
- React-এ: useState + event handler function
- useState = component এর local state
  loading = spinner দেখানো/লুকানো
  error   = error message দেখানো
  showPassword = eye toggle
- useNavigate = page redirect করা
- useAuth = login function নেওয়া (Context থেকে)
- api.post = Axios দিয়ে POST request
================================================================
*/
export default function Login() {
    // State variables
    const [email, setEmail]               = useState('');
    const [password, setPassword]         = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [loading, setLoading]           = useState(false);
    const [error, setError]               = useState('');

    const { login }  = useAuth();
    const navigate   = useNavigate();

    /*
    LEARNING: handleSubmit
    e.preventDefault() = form default submit বন্ধ করে
    try/catch = error handle করা
    api.post() = Axios POST request
    */
    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!email || !password) {
            setError('Email এবং Password দাও।');
            return;
        }

        setLoading(true);
        setError('');

        try {
            const res = await api.post('/employee/login', { email, password });
            login(res.data.token, res.data.employee);
            navigate('/dashboard');
        } catch (err) {
            setError(err.response?.data?.message || 'Login failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-vh-100 d-flex align-items-center justify-content-center"
             style={{ background: 'linear-gradient(135deg, #1a1a2e, #16213e, #0f3460)' }}>
            <div className="card shadow-lg border-0" style={{ width: '420px', borderRadius: '16px' }}>
                <div className="card-body p-4">

                    {/* Brand */}
                    <div className="text-center mb-4">
                        <div className="mx-auto mb-3 d-flex align-items-center justify-content-center"
                             style={{ width:64, height:64, background:'linear-gradient(135deg,#0d6efd,#0a58ca)',
                                      borderRadius:16, fontSize:28, color:'white' }}>
                            <i className="bi bi-building-gear"></i>
                        </div>
                        <h4 className="fw-bold mb-0">First ERP System</h4>
                        <small className="text-muted">Sign in to your account</small>
                    </div>

                    {/* Error alert */}
                    {error && (
                        <div className="alert alert-danger py-2">{error}</div>
                    )}

                    {/* Form */}
                    <form onSubmit={handleSubmit}>
                        {/* Email */}
                        <div className="mb-3">
                            <label className="form-label fw-semibold">
                                <i className="bi bi-envelope me-1"></i>Email
                            </label>
                            <input
                                type="email"
                                className="form-control form-control-lg"
                                placeholder="email@erp.com"
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                autoComplete="email"
                            />
                        </div>

                        {/* Password with eye toggle */}
                        {/*
                        LEARNING: eye toggle
                        jQuery-এ: input.attr('type', 'text')
                        React-এ: type={showPassword ? 'text' : 'password'}
                        showPassword state toggle করলেই re-render হয়
                        */}
                        <div className="mb-4">
                            <label className="form-label fw-semibold">
                                <i className="bi bi-lock me-1"></i>Password
                            </label>
                            <div className="input-group">
                                <input
                                    type={showPassword ? 'text' : 'password'}
                                    className="form-control form-control-lg"
                                    placeholder="Enter your password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    autoComplete="current-password"
                                />
                                <button
                                    type="button"
                                    className="btn btn-outline-secondary"
                                    onClick={() => setShowPassword(!showPassword)}>
                                    <i className={`bi bi-eye${showPassword ? '-slash' : ''}`}></i>
                                </button>
                            </div>
                        </div>

                        {/* Submit */}
                        <button
                            type="submit"
                            className="btn btn-primary btn-lg w-100"
                            disabled={loading}>
                            {loading && (
                                <span className="spinner-border spinner-border-sm me-2"></span>
                            )}
                            <i className="bi bi-box-arrow-in-right me-1"></i>
                            Login
                        </button>
                    </form>

                    <div className="text-center mt-3">
                        <small className="text-muted">ERP v1.0 &copy; 2026 Himel Sarker</small>
                    </div>
                </div>
            </div>
        </div>
    );
}
