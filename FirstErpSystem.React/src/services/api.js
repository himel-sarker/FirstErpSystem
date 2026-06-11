import axios from 'axios';

/*
================================================================
api.js — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Axios = jQuery AJAX এর modern alternative
- axios.create() = base URL সহ একটা instance তৈরি
- interceptors = প্রতিটা request/response এ automatically কাজ করে
- Request interceptor = প্রতিটা request-এ JWT token add করে
- Response interceptor = 401 এলে automatically logout করে
- এই pattern কে "Axios Instance" বলে — real project-এ সবাই এটা use করে
================================================================
*/

// Base API instance
const api = axios.create({
    baseURL: 'http://localhost:5123/api',
    headers: {
        'Content-Type': 'application/json'
    }
});

/*
LEARNING: Request Interceptor
প্রতিটা request যাওয়ার আগে এই function চলে
localStorage থেকে token নিয়ে header-এ add করে
jQuery AJAX-এ manually করতে হতো:
headers: { 'Authorization': 'Bearer ' + getToken() }
Axios interceptor-এ একবার লিখলেই সব request-এ কাজ করে
*/
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('erp_token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

/*
LEARNING: Response Interceptor
প্রতিটা response আসার পর এই function চলে
401 = token expired → localStorage clear → login page-এ redirect
*/
api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('erp_token');
            localStorage.removeItem('erp_user');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default api;
