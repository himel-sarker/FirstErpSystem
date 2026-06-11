import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

/*
================================================================
main.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Entry point = React app এর starting point
- ReactDOM.createRoot = React 18 এর new API
- <App /> = root component render হয়
- Bootstrap CSS = globally import করা
  সব component-এ Bootstrap classes available হবে
- bootstrap-icons = সব bi bi-* icons available হবে
- index.css = custom global styles
================================================================
*/

// Bootstrap + Icons (order matters!)
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap-icons/font/bootstrap-icons.css';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <App />
    </React.StrictMode>
);
