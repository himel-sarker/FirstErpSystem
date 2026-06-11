import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import Navbar from '../components/Navbar';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';

/*
================================================================
Dashboard.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- useEffect = component load হলে data fetch করে
  jQuery-এ: $(document).ready(function(){ loadEmployees(); })
  React-এ:  useEffect(() => { loadData(); }, [])
  [] = dependency array খালি = শুধু একবার চলবে
- Promise.all = একসাথে multiple API call করা
  faster than sequential calls
- useState দিয়ে stats store করি
- Re-render = state change হলে UI automatically update
================================================================
*/
export default function Dashboard() {
    const { user } = useAuth();

    // State for stats
    const [stats, setStats] = useState({
        totalEmployees : 0,
        totalProducts  : 0,
        lowStockCount  : 0,
        pendingPOs     : 0,
        pendingSOs     : 0,
        totalDepts     : 0
    });
    const [loading, setLoading] = useState(true);
    const [employees, setEmployees] = useState([]);

    /*
    LEARNING: useEffect
    - Component mount হলে একবার চলে ([] dependency)
    - Promise.all = দুটো API call একসাথে
    - Faster: 2 calls parallel vs sequential
    */
    useEffect(() => {
        loadDashboardData();
    }, []);

    const loadDashboardData = async () => {
        try {
            const [empRes, prodRes, poRes, soRes] = await Promise.all([
                api.get('/employee'),
                api.get('/product'),
                api.get('/purchaseorder'),
                api.get('/salesorder')
            ]);

            const employees = empRes.data;
            const products  = prodRes.data;
            const pos       = poRes.data;
            const sos       = soRes.data;

            setEmployees(employees);
            setStats({
                totalEmployees : employees.length,
                totalProducts  : products.length,
                lowStockCount  : products.filter(p => p.isLowStock).length,
                pendingPOs     : pos.filter(p => p.status === 'Draft' || p.status === 'Approved').length,
                pendingSOs     : sos.filter(s => s.status !== 'Paid').length,
                totalDepts     : [...new Set(employees.map(e => e.department))].length
            });
        } catch (err) {
            console.error('Dashboard load error:', err);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <>
                <Navbar />
                <div className="d-flex justify-content-center align-items-center" style={{ height:'60vh' }}>
                    <div className="spinner-border text-primary"></div>
                </div>
            </>
        );
    }

    return (
        <>
            <Navbar />
            <div className="container-fluid mt-4 px-4">

                {/* Welcome */}
                <div className="mb-4">
                    <h4 className="fw-bold mb-0">
                        Welcome back, {user?.fullName}! 👋
                    </h4>
                    <small className="text-muted">Here's what's happening in your ERP today.</small>
                </div>

                {/* Stat Cards */}
                {/*
                LEARNING: map() = array loop করে JSX render করা
                jQuery-এ: forEach + string concatenation
                React-এ: array.map() + JSX — cleaner and safer
                */}
                <div className="row g-3 mb-4">
                    {[
                        { label:'Total Employees', value:stats.totalEmployees, icon:'bi-people',        color:'primary' },
                        { label:'Departments',     value:stats.totalDepts,     icon:'bi-diagram-3',    color:'success' },
                        { label:'Products',        value:stats.totalProducts,  icon:'bi-box-seam',     color:'info'    },
                        { label:'Low Stock',       value:stats.lowStockCount,  icon:'bi-exclamation-triangle', color:'warning' },
                        { label:'Pending POs',     value:stats.pendingPOs,     icon:'bi-cart',         color:'secondary'},
                        { label:'Pending Sales',   value:stats.pendingSOs,     icon:'bi-bag',          color:'danger'  },
                    ].map((stat, i) => (
                        <div className="col-md-2 col-sm-4" key={i}>
                            <div className={`card text-white bg-${stat.color} h-100 border-0`}
                                 style={{ borderRadius:12, boxShadow:'0 4px 12px rgba(0,0,0,0.12)' }}>
                                <div className="card-body d-flex justify-content-between align-items-center">
                                    <div>
                                        <div style={{ fontSize:13, opacity:.9 }}>{stat.label}</div>
                                        <div style={{ fontSize:28, fontWeight:700, lineHeight:1 }}>{stat.value}</div>
                                    </div>
                                    <i className={`bi ${stat.icon}`} style={{ fontSize:'2.5rem', opacity:.3 }}></i>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Quick Links */}
                <div className="row g-3 mb-4">
                    <div className="col-md-4">
                        <div className="card border-0 h-100" style={{ borderRadius:12, boxShadow:'0 2px 8px rgba(0,0,0,0.08)' }}>
                            <div className="card-body">
                                <h6 className="fw-bold mb-3"><i className="bi bi-lightning me-2"></i>Quick Actions</h6>
                                <div className="d-flex flex-column gap-2">
                                    <Link to="/products" className="btn btn-outline-primary btn-sm text-start">
                                        <i className="bi bi-plus-lg me-2"></i>Add Product
                                    </Link>
                                    <Link to="/purchase-orders" className="btn btn-outline-success btn-sm text-start">
                                        <i className="bi bi-cart-plus me-2"></i>New Purchase Order
                                    </Link>
                                    <Link to="/sales-orders" className="btn btn-outline-info btn-sm text-start">
                                        <i className="bi bi-bag-plus me-2"></i>New Sales Order
                                    </Link>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Employee table */}
                    <div className="col-md-8">
                        <div className="card border-0" style={{ borderRadius:12, boxShadow:'0 2px 8px rgba(0,0,0,0.08)' }}>
                            <div className="card-header bg-white border-bottom py-3">
                                <h6 className="fw-bold mb-0"><i className="bi bi-people me-2"></i>Employees</h6>
                            </div>
                            <div className="card-body p-0">
                                <table className="table table-hover mb-0">
                                    <thead className="table-dark">
                                        <tr>
                                            <th className="ps-3">#</th>
                                            <th>Name</th>
                                            <th>Department</th>
                                            <th>Role</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {employees.map((emp, i) => (
                                            <tr key={emp.id}>
                                                <td className="ps-3 text-muted">{i + 1}</td>
                                                <td>
                                                    <div className="d-flex align-items-center gap-2">
                                                        <div style={{
                                                            width:30, height:30, borderRadius:'50%',
                                                            background:'linear-gradient(135deg,#0d6efd,#0a58ca)',
                                                            color:'white', display:'flex',
                                                            alignItems:'center', justifyContent:'center',
                                                            fontSize:13, fontWeight:600
                                                        }}>
                                                            {emp.fullName.charAt(0).toUpperCase()}
                                                        </div>
                                                        <strong>{emp.fullName}</strong>
                                                    </div>
                                                </td>
                                                <td>{emp.department}</td>
                                                <td>
                                                    <span className={`badge bg-${
                                                        emp.role === 'Admin' ? 'danger' :
                                                        emp.role === 'Manager' ? 'warning' : 'success'
                                                    }`}>{emp.role}</span>
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>

            </div>
        </>
    );
}
