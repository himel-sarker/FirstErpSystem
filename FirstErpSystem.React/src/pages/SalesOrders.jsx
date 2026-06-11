import { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';
import api from '../services/api';

/*
================================================================
SalesOrders.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Sales Order list + Create + Confirm + Invoice + Pay
- PurchaseOrders.jsx এর মতোই structure
- Extra: Payment modal with method selection
================================================================
*/
export default function SalesOrders() {
    const [orders, setOrders]           = useState([]);
    const [products, setProducts]       = useState([]);
    const [loading, setLoading]         = useState(true);
    const [showModal, setShowModal]     = useState(false);
    const [showPayModal, setShowPayModal] = useState(false);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [error, setError]             = useState('');
    const [success, setSuccess]         = useState('');

    const [form, setForm] = useState({
        customerName: '', customerEmail: '',
        customerPhone: '', notes: ''
    });

    const [lines, setLines] = useState([
        { productId: '', quantity: '', unitPrice: '' }
    ]);

    const [payForm, setPayForm] = useState({
        paymentMethod: 'Cash', transactionId: ''
    });

    useEffect(() => {
        loadOrders();
        loadProducts();
    }, []);

    const loadOrders = async () => {
        try {
            const res = await api.get('/salesorder');
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

    const handleLineChange = (index, field, value) => {
        const updated = [...lines];
        updated[index][field] = value;
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
        if (lines.length === 1) return;
        setLines(lines.filter((_, i) => i !== index));
    };

    const total = lines.reduce((sum, l) => {
        return sum + (parseFloat(l.quantity || 0) * parseFloat(l.unitPrice || 0));
    }, 0);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            await api.post('/salesorder', {
                ...form,
                lines: lines.map(l => ({
                    productId: parseInt(l.productId),
                    quantity : parseInt(l.quantity),
                    unitPrice: parseFloat(l.unitPrice)
                }))
            });
            setSuccess('Sales order created!');
            setShowModal(false);
            setForm({ customerName:'', customerEmail:'', customerPhone:'', notes:'' });
            setLines([{ productId:'', quantity:'', unitPrice:'' }]);
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to create order');
        }
    };

    const handleConfirm = async (id) => {
        if (!window.confirm('Confirm this order?')) return;
        try {
            await api.put(`/salesorder/${id}/confirm`);
            setSuccess('Order confirmed!');
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Confirm failed');
        }
    };

    const handleInvoice = async (id) => {
        if (!window.confirm('Generate invoice? Email will be sent to customer.')) return;
        try {
            await api.put(`/salesorder/${id}/invoice`);
            setSuccess('Invoice generated! Email sent to customer.');
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Invoice failed');
        }
    };

    const openPayModal = (order) => {
        setSelectedOrder(order);
        setShowPayModal(true);
    };

    const handlePayment = async (e) => {
        e.preventDefault();
        setError('');
        try {
            await api.put(`/salesorder/${selectedOrder.id}/pay`, payForm);
            setSuccess('Payment recorded! Stock updated. SMS sent.');
            setShowPayModal(false);
            setPayForm({ paymentMethod:'Cash', transactionId:'' });
            loadOrders();
        } catch (err) {
            setError(err.response?.data?.message || 'Payment failed');
        }
    };

    const getStatusBadge = (status) => {
        const colors = {
            Draft:'secondary', Confirmed:'primary',
            Invoiced:'warning', Paid:'success'
        };
        return <span className={`badge bg-${colors[status] || 'secondary'}`}>{status}</span>;
    };

    return (
        <>
            <Navbar />
            <div className="container-fluid mt-4 px-4">

                {/* Header */}
                <div className="d-flex justify-content-between align-items-center mb-4">
                    <div>
                        <h4 className="fw-bold mb-0"><i className="bi bi-bag me-2"></i>Sales Orders</h4>
                        <small className="text-muted">Manage customer sales orders</small>
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
                                        <th>Customer</th>
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
                                            <td>{o.customerName}</td>
                                            <td className="fw-semibold">৳{o.totalAmount.toLocaleString()}</td>
                                            <td>{new Date(o.orderDate).toLocaleDateString()}</td>
                                            <td>{getStatusBadge(o.status)}</td>
                                            <td className="text-center">
                                                {o.status === 'Draft' && (
                                                    <button className="btn btn-primary btn-sm me-1"
                                                            onClick={() => handleConfirm(o.id)}>
                                                        <i className="bi bi-check-lg me-1"></i>Confirm
                                                    </button>
                                                )}
                                                {o.status === 'Confirmed' && (
                                                    <button className="btn btn-warning btn-sm me-1"
                                                            onClick={() => handleInvoice(o.id)}>
                                                        <i className="bi bi-receipt me-1"></i>Invoice
                                                    </button>
                                                )}
                                                {o.status === 'Invoiced' && (
                                                    <button className="btn btn-success btn-sm"
                                                            onClick={() => openPayModal(o)}>
                                                        <i className="bi bi-credit-card me-1"></i>Pay
                                                    </button>
                                                )}
                                                {o.status === 'Paid' && (
                                                    <span className="text-muted small">
                                                        <i className="bi bi-check-circle-fill text-success me-1"></i>
                                                        Completed
                                                    </span>
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
                                    <i className="bi bi-bag-plus me-2"></i>New Sales Order
                                </h5>
                                <button className="btn-close btn-close-white"
                                        onClick={() => setShowModal(false)}></button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger py-2">{error}</div>}
                                    <h6 className="fw-bold mb-3">Customer Information</h6>
                                    <div className="row g-2 mb-3">
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Customer Name</label>
                                            <input name="customerName" className="form-control"
                                                   value={form.customerName} onChange={handleChange} required />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Email</label>
                                            <input name="customerEmail" type="email" className="form-control"
                                                   value={form.customerEmail} onChange={handleChange} required />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Phone</label>
                                            <input name="customerPhone" className="form-control"
                                                   value={form.customerPhone} onChange={handleChange} />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Notes</label>
                                            <input name="notes" className="form-control"
                                                   value={form.notes} onChange={handleChange} />
                                        </div>
                                    </div>

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
                                                        <option key={p.id} value={p.id}>
                                                            {p.name} (Stock: {p.stockQuantity})
                                                        </option>
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
                                                <button type="button"
                                                        className="btn btn-outline-danger btn-sm w-100"
                                                        onClick={() => removeLine(index)}>
                                                    <i className="bi bi-trash"></i>
                                                </button>
                                            </div>
                                        </div>
                                    ))}

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

            {/* Payment Modal */}
            {showPayModal && (
                <div className="modal show d-block" style={{ background:'rgba(0,0,0,0.5)' }}>
                    <div className="modal-dialog">
                        <div className="modal-content border-0" style={{ borderRadius:12 }}>
                            <div className="modal-header bg-success text-white"
                                 style={{ borderRadius:'12px 12px 0 0' }}>
                                <h5 className="modal-title">
                                    <i className="bi bi-credit-card me-2"></i>
                                    Record Payment — {selectedOrder?.orderNo}
                                </h5>
                                <button className="btn-close btn-close-white"
                                        onClick={() => setShowPayModal(false)}></button>
                            </div>
                            <form onSubmit={handlePayment}>
                                <div className="modal-body">
                                    <div className="alert alert-info py-2">
                                        Total Amount: <strong>৳{selectedOrder?.totalAmount.toLocaleString()}</strong>
                                    </div>
                                    <div className="mb-3">
                                        <label className="form-label fw-semibold">Payment Method</label>
                                        <select className="form-select"
                                                value={payForm.paymentMethod}
                                                onChange={e => setPayForm({...payForm, paymentMethod: e.target.value})}>
                                            <option value="Cash">Cash</option>
                                            <option value="Bank Transfer">Bank Transfer</option>
                                            <option value="SSLCommerz">SSLCommerz</option>
                                            <option value="bKash">bKash</option>
                                            <option value="Nagad">Nagad</option>
                                        </select>
                                    </div>
                                    <div className="mb-3">
                                        <label className="form-label fw-semibold">Transaction ID</label>
                                        <input className="form-control"
                                               value={payForm.transactionId}
                                               onChange={e => setPayForm({...payForm, transactionId: e.target.value})}
                                               placeholder="e.g. TXN-12345" />
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary"
                                            onClick={() => setShowPayModal(false)}>Cancel</button>
                                    <button type="submit" className="btn btn-success">
                                        <i className="bi bi-check-lg me-1"></i>Confirm Payment
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
