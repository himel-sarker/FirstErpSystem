import { createContext, useContext, useState } from 'react';

/*
================================================================
AuthContext — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Context API = global state management
- jQuery dashboard-এ localStorage directly access করতাম
- React-এ Context দিয়ে করি — যেকোনো component থেকে
  user info আর token access করা যায়
- createContext() = context তৈরি
- useContext() = context use করা
- Provider = context এর value সব child-এ দেওয়া
FLOW:
  App.jsx → AuthProvider wrap করে
  যেকোনো component → useAuth() hook দিয়ে user পায়
================================================================
*/

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    /*
    LEARNING: useState দিয়ে user state manage করি
    Initial value = localStorage থেকে নেওয়া
    Page refresh করলেও user logged in থাকে
    */
    const [user, setUser] = useState(() => {
        const saved = localStorage.getItem('erp_user');
        return saved ? JSON.parse(saved) : null;
    });

    // Login = token + user save করা
    const login = (token, userData) => {
        localStorage.setItem('erp_token', token);
        localStorage.setItem('erp_user', JSON.stringify(userData));
        setUser(userData);
    };

    // Logout = token + user remove করা
    const logout = () => {
        localStorage.removeItem('erp_token');
        localStorage.removeItem('erp_user');
        setUser(null);
    };

    return (
        <AuthContext.Provider value={{ user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
}

/*
LEARNING: Custom Hook
useAuth() = useContext(AuthContext) এর shortcut
যেকোনো component-এ:
const { user, login, logout } = useAuth();
*/
export function useAuth() {
    return useContext(AuthContext);
}
