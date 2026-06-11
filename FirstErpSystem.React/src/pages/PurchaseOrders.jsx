import { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';
import api from '../services/api';

/*
================================================================
PurchaseOrders.jsx — Added By Himel Sarker 09-06-2026
LEARNING FLOW:
- Purchase Order list + Create + Approve + Receive
- Multi-line form = dynamic lines array
  lines.map() দিয়ে render করি
  addLine() = নতুন line add করা
  removeLine() = line remove করা
  handleLineChange() = line field update করা
================================================================
*/
export default function PurchaseOrders() {
    const [orders, setOrders]       = useState([]);
    const [products, setProducts]   = useState([]);
    const [loading, setLoading]     = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [error, setError]         = useState('');
    const [success, setSuccess]     = useState('');

    // Form state
    const [form, setForm] = useState({
        supplierName: '', supplierEmail: '',
        supplierPhone: '', notes: ''
    });

    /*
    LEARNING: Dynamic lines array
    প্রতিটা line = একটা object { productId, quantity, unitPrice }
    Initial = একটা empty line
    */
    const [lines, setLines] = useState([
        { productId: '', quantity: '', unitPrice: '' }
    ]);

    useEffect(() => {
        loadOrders();
        loadProducts();
    }, []);

    const loadOrders = async () => {
        try {
            const res = await api.get('/purchaseorder');
            setOrders(res.data);
        } catch (err) {
            setError('Orders load failed');
        } finally {
            setLoading(false);
        }
    };

    const loadProducts = async () => {
        try {
            const res = await api.get('/product');
            setProducts(res.data);
        } catch (err) {}
    };

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    /*
    LEARNING: Dynamic line handling
    index = কোন line update হবে সেটা
    spread operator দিয়ে array copy করি
    তারপর সেই index update করি
    */
    const handleLineChange = (index, field, value) => {
        const updated = [...lines];
        updated[index][field] = value;

        // Product select করলে auto unit price set
        if (field === 'productId') {
            const product = products.find(p => p.id === parseInt(value));
            if (product) updated[index].unitPrice = product.unitPrice;
        }
        setLines(updated);
    };

    const addLine = () => {
        setLines([...lines, { productId: '', quantity: '', unitPrice: '' }]);
    };

    const removeLine = (index) => {
        if (lines.length === 1) return; // কমপক্ষে একটা line থাকবে
        setLines(lines.filter((_, i) => i !== index));
    };

    // Calculate total
    const total = lines.reduce((sum, l) => {
        return sum + (parseFloat(l.quantity || 0) * parseFloat(l.unitPrice || 0));
    }, 0);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            await api.post('/purchaseorder', {
                ...form,
                lines: lines.map(l => ({
                    productId: parseInt(l.productId),
                    quantity : parseInt(l.quantity),
                    unitPrice: parseFloat(l.unitPrice)
                }))
            });
            setSuccess('Purchase order created!');
            setShowModal(false);
            setForm({ supplierName:'', supplierEmail:'', supplierPhone:'', notes:'' });
            setLines([{ productId:'', quantity:'', unitPrice:'' }]);
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to create order');
        }
    };

    const handleApprove = async (id) => {
        if (!window.confirm('Approve this order?')) return;
        try {
            await api.put(`/purchaseorder/${id}/approve`);
            setSuccess('Order approved!');
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Approve failed');
        }
    };

    const handleReceive = async (id) => {
        if (!window.confirm('Mark as received? Stock will be updated.')) return;
        try {
            await api.put(`/purchaseorder/${id}/receive`);
            setSuccess('Order received! Stock updated.');
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Receive failed');
        }
    };

    const getStatusBadge = (status) => {
        const colors = { Draft:'secondary', Approved:'primary', Received:'success' };
        return <span className={`badge bg-${colors[status] || 'secondary'}`}>{status}</span>;
    };

    return (
        <>
            <Navbar />
            <div className="container-fluid mt-4 px-4">

                {/* Header */}
                <div className="d-flex justify-content-between align-items-center mb-4">
                    <div>
                        <h4 className="fw-bold mb-0"><i className="bi bi-cart me-2"></i>Purchase Orders</h4>
                        <small className="text-muted">Manage supplier purchase orders</small>
                    </div>
                    <button className="btn btn-primary" onClick={() => setShowModal(true)}>
                        <i className="bi bi-plus-lg me-1"></i>New Order
                    </button>
                </div>

                {/* Alerts */}
                {error   && <div className="alert alert-danger alert-dismissible">
                    {error}<button className="btn-close" onClick={() => setError('')}></button></div>}
                {success && <div className="alert alert-success alert-dismissible">
                    {success}<button className="btn-close" onClick={() => setSuccess('')}></button></div>}

                {/* Table */}
                <div className="card border-0" style={{ borderRadius:12, boxShadow:'0 2px 8px rgba(0,0,0,0.08)' }}>
                    <div className="card-body p-0">
                        <div className="table-responsive">
                            <table className="table table-hover mb-0">
                                <thead className="table-dark">
                                    <tr>
                                        <th className="ps-3">Order No</th>
                                        <th>Supplier</th>
                                        <th>Total</th>
                                        <th>Date</th>
                                        <th>Status</th>
                                        <th className="text-center">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {loading ? (
                                        <tr><td colSpan="6" className="text-center py-4">
                                            <div className="spinner-border spinner-border-sm"></div>
                                        </td></tr>
                                    ) : orders.map(o => (
                                        <tr key={o.id}>
                                            <td className="ps-3 fw-bold">{o.orderNo}</td>
                                            <td>{o.supplierName}</td>
                                            <td className="fw-semibold">৳{o.totalAmount.toLocaleString()}</td>
                                            <td>{new Date(o.orderDate).toLocaleDateString()}</td>
                                            <td>{getStatusBadge(o.status)}</td>
                                            <td className="text-center">
                                                {o.status === 'Draft' && (
                                                    <button className="btn btn-primary btn-sm me-1"
                                                            onClick={() => handleApprove(o.id)}>
                                                        <i className="bi bi-check-lg me-1"></i>Approve
                                                    </button>
                                                )}
                                                {o.status === 'Approved' && (
                                                    <button className="btn btn-success btn-sm"
                                                            onClick={() => handleReceive(o.id)}>
                                                        <i className="bi bi-box-arrow-in-down me-1"></i>Receive
                                                    </button>
                                                )}
                                                {o.status === 'Received' && (
                                                    <span className="text-muted small">Completed</span>
                                                )}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            {/* Create Modal */}
            {showModal && (
                <div className="modal show d-block" style={{ background:'rgba(0,0,0,0.5)' }}>
                    <div className="modal-dialog modal-lg">
                        <div className="modal-content border-0" style={{ borderRadius:12 }}>
                            <div className="modal-header bg-primary text-white"
                                 style={{ borderRadius:'12px 12px 0 0' }}>
                                <h5 className="modal-title">
                                    <i className="bi bi-cart-plus me-2"></i>New Purchase Order
                                </h5>
                                <button className="btn-close btn-close-white"
                                        onClick={() => setShowModal(false)}></button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger py-2">{error}</div>}

                                    {/* Supplier info */}
                                    <h6 className="fw-bold mb-3">Supplier Information</h6>
                                    <div className="row g-2 mb-3">
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Supplier Name</label>
                                            <input name="supplierName" className="form-control"
                                                   value={form.supplierName} onChange={handleChange} required />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Email</label>
                                            <input name="supplierEmail" type="email" className="form-control"
                                                   value={form.supplierEmail} onChange={handleChange} required />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Phone</label>
                                            <input name="supplierPhone" className="form-control"
                                                   value={form.supplierPhone} onChange={handleChange} />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Notes</label>
                                            <input name="notes" className="form-control"
                                                   value={form.notes} onChange={handleChange} />
                                        </div>
                                    </div>

                                    {/* Order lines */}
                                    <div className="d-flex justify-content-between align-items-center mb-2">
                                        <h6 className="fw-bold mb-0">Order Items</h6>
                                        <button type="button" className="btn btn-outline-primary btn-sm"
                                                onClick={addLine}>
                                            <i className="bi bi-plus-lg me-1"></i>Add Line
                                        </button>
                                    </div>

                                    {lines.map((line, index) => (
                                        <div key={index} className="row g-2 mb-2 align-items-end">
                                            <div className="col-5">
                                                <label className="form-label fw-semibold">Product</label>
                                                <select className="form-select"
                                                        value={line.productId}
                                                        onChange={e => handleLineChange(index, 'productId', e.target.value)}
                                                        required>
                                                    <option value="">Select product</option>
                                                    {products.map(p => (
                                                        <option key={p.id} value={p.id}>{p.name} ({p.code})</option>
                                                    ))}
                                                </select>
                                            </div>
                                            <div className="col-2">
                                                <label className="form-label fw-semibold">Qty</label>
                                                <input type="number" className="form-control"
                                                       value={line.quantity} min="1"
                                                       onChange={e => handleLineChange(index, 'quantity', e.target.value)}
                                                       required />
                                            </div>
                                            <div className="col-3">
                                                <label className="form-label fw-semibold">Unit Price</label>
                                                <input type="number" className="form-control"
                                                       value={line.unitPrice}
                                                       onChange={e => handleLineChange(index, 'unitPrice', e.target.value)}
                                                       required />
                                            </div>
                                            <div className="col-2">
                                                <button type="button" className="btn btn-outline-danger btn-sm w-100"
                                                        onClick={() => removeLine(index)}>
                                                    <i className="bi bi-trash"></i>
                                                </button>
                                            </div>
                                        </div>
                                    ))}

                                    {/* Total */}
                                    <div className="text-end mt-3">
                                        <strong>Total: ৳{total.toLocaleString()}</strong>
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary"
                                            onClick={() => setShowModal(false)}>Cancel</button>
                                    <button type="submit" className="btn btn-primary">
                                        <i className="bi bi-check-lg me-1"></i>Create Order
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}
