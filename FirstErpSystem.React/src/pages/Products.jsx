import { useState, useEffect } from 'react';
import Navbar from '../components/Navbar';
import api from '../services/api';

/*
================================================================
Products.jsx — Added By Himel Sarkar 09-06-2026
LEARNING FLOW:
- Product list + Add + Stock IN/OUT
- useState array = products list store করা
- Modal state = showModal true/false দিয়ে control
- jQuery-এ: bootstrap.Modal.show()
  React-এ: useState দিয়ে conditional render
- form state = controlled inputs
  value={form.name} onChange = React way
================================================================
*/
export default function Products() {
    const [products, setProducts]   = useState([]);
    const [loading, setLoading]     = useState(true);
    const [showModal, setShowModal] = useState(false);
    const [showStockModal, setShowStockModal] = useState(false);
    const [selectedProduct, setSelectedProduct] = useState(null);
    const [stockType, setStockType] = useState('in');
    const [error, setError]         = useState('');
    const [success, setSuccess]     = useState('');

    // Form state
    const [form, setForm] = useState({
        name: '', code: '', description: '',
        category: '', unitPrice: '', stockQuantity: '',
        reorderLevel: '', unit: 'pcs'
    });

    // Stock form state
    const [stockForm, setStockForm] = useState({
        quantity: '', referenceNo: '', remarks: ''
    });

    useEffect(() => { loadProducts(); }, []);

    const loadProducts = async () => {
        try {
            const res = await api.get('/product');
            setProducts(res.data);
        } catch (err) {
            setError('Products load failed');
        } finally {
            setLoading(false);
        }
    };

    /*
    LEARNING: handleChange pattern
    [e.target.name] = dynamic key
    ...form = spread operator — existing values copy করে
    শুধু changed field update হয়
    */
    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value });
    };

    const handleStockChange = (e) => {
        setStockForm({ ...stockForm, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            await api.post('/product', {
                ...form,
                unitPrice    : parseFloat(form.unitPrice),
                stockQuantity: parseInt(form.stockQuantity),
                reorderLevel : parseInt(form.reorderLevel),
                isActive     : true,
                createdAt    : new Date().toISOString()
            });
            setSuccess('Product added successfully!');
            setShowModal(false);
            setForm({ name:'', code:'', description:'', category:'',
                      unitPrice:'', stockQuantity:'', reorderLevel:'', unit:'pcs' });
            loadProducts();
        } catch (err) {
            setError(err.response?.data?.message || 'Failed to add product');
        }
    };

    const handleStockSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            const endpoint = stockType === 'in'
                ? `/product/${selectedProduct.id}/stock-in`
                : `/product/${selectedProduct.id}/stock-out`;

            await api.post(endpoint, {
                quantity    : parseInt(stockForm.quantity),
                referenceNo : stockForm.referenceNo,
                remarks     : stockForm.remarks
            });
            setSuccess(`Stock ${stockType === 'in' ? 'IN' : 'OUT'} recorded!`);
            setShowStockModal(false);
            setStockForm({ quantity:'', referenceNo:'', remarks:'' });
            loadProducts();
        } catch (err) {
            setError(err.response?.data?.message || 'Stock update failed');
        }
    };

    const openStockModal = (product, type) => {
        setSelectedProduct(product);
        setStockType(type);
        setShowStockModal(true);
    };

    return (
        <>
            <Navbar />
            <div className="container-fluid mt-4 px-4">

                {/* Header */}
                <div className="d-flex justify-content-between align-items-center mb-4">
                    <div>
                        <h4 className="fw-bold mb-0"><i className="bi bi-box-seam me-2"></i>Inventory</h4>
                        <small className="text-muted">Manage products and stock levels</small>
                    </div>
                    <button className="btn btn-primary" onClick={() => setShowModal(true)}>
                        <i className="bi bi-plus-lg me-1"></i>Add Product
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
                                        <th className="ps-3">#</th>
                                        <th>Code</th>
                                        <th>Name</th>
                                        <th>Category</th>
                                        <th>Unit Price</th>
                                        <th>Stock</th>
                                        <th>Status</th>
                                        <th className="text-center">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {loading ? (
                                        <tr><td colSpan="8" className="text-center py-4">
                                            <div className="spinner-border spinner-border-sm"></div>
                                        </td></tr>
                                    ) : products.map((p, i) => (
                                        <tr key={p.id}>
                                            <td className="ps-3 text-muted">{i + 1}</td>
                                            <td><span className="badge bg-light text-dark border">{p.code}</span></td>
                                            <td><strong>{p.name}</strong></td>
                                            <td>{p.category}</td>
                                            <td className="fw-semibold">৳{p.unitPrice.toLocaleString()}</td>
                                            <td>
                                                <span className={`badge bg-${p.isLowStock ? 'danger' : 'success'}`}>
                                                    {p.stockQuantity} {p.unit}
                                                </span>
                                            </td>
                                            <td>
                                                {p.isLowStock
                                                    ? <span className="badge bg-warning">Low Stock</span>
                                                    : <span className="badge bg-success">OK</span>
                                                }
                                            </td>
                                            <td className="text-center">
                                                <button className="btn btn-success btn-sm me-1"
                                                        onClick={() => openStockModal(p, 'in')}
                                                        title="Stock IN">
                                                    <i className="bi bi-plus-lg"></i>
                                                </button>
                                                <button className="btn btn-warning btn-sm"
                                                        onClick={() => openStockModal(p, 'out')}
                                                        title="Stock OUT">
                                                    <i className="bi bi-dash-lg"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            {/* Add Product Modal */}
            {showModal && (
                <div className="modal show d-block" style={{ background:'rgba(0,0,0,0.5)' }}>
                    <div className="modal-dialog">
                        <div className="modal-content border-0" style={{ borderRadius:12 }}>
                            <div className="modal-header bg-primary text-white" style={{ borderRadius:'12px 12px 0 0' }}>
                                <h5 className="modal-title"><i className="bi bi-box-seam me-2"></i>Add Product</h5>
                                <button className="btn-close btn-close-white" onClick={() => setShowModal(false)}></button>
                            </div>
                            <form onSubmit={handleSubmit}>
                                <div className="modal-body">
                                    {error && <div className="alert alert-danger py-2">{error}</div>}
                                    <div className="row g-2">
                                        <div className="col-8">
                                            <label className="form-label fw-semibold">Name</label>
                                            <input name="name" className="form-control" value={form.name}
                                                   onChange={handleChange} required />
                                        </div>
                                        <div className="col-4">
                                            <label className="form-label fw-semibold">Code</label>
                                            <input name="code" className="form-control" value={form.code}
                                                   onChange={handleChange} placeholder="PRD-004" required />
                                        </div>
                                        <div className="col-12">
                                            <label className="form-label fw-semibold">Description</label>
                                            <input name="description" className="form-control"
                                                   value={form.description} onChange={handleChange} />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Category</label>
                                            <input name="category" className="form-control"
                                                   value={form.category} onChange={handleChange} required />
                                        </div>
                                        <div className="col-6">
                                            <label className="form-label fw-semibold">Unit</label>
                                            <select name="unit" className="form-select"
                                                    value={form.unit} onChange={handleChange}>
                                                <option value="pcs">pcs</option>
                                                <option value="kg">kg</option>
                                                <option value="ltr">ltr</option>
                                                <option value="box">box</option>
                                            </select>
                                        </div>
                                        <div className="col-4">
                                            <label className="form-label fw-semibold">Unit Price (৳)</label>
                                            <input name="unitPrice" type="number" className="form-control"
                                                   value={form.unitPrice} onChange={handleChange} required />
                                        </div>
                                        <div className="col-4">
                                            <label className="form-label fw-semibold">Initial Stock</label>
                                            <input name="stockQuantity" type="number" className="form-control"
                                                   value={form.stockQuantity} onChange={handleChange} required />
                                        </div>
                                        <div className="col-4">
                                            <label className="form-label fw-semibold">Reorder Level</label>
                                            <input name="reorderLevel" type="number" className="form-control"
                                                   value={form.reorderLevel} onChange={handleChange} required />
                                        </div>
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary"
                                            onClick={() => setShowModal(false)}>Cancel</button>
                                    <button type="submit" className="btn btn-primary">
                                        <i className="bi bi-check-lg me-1"></i>Save Product
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            )}

            {/* Stock Modal */}
            {showStockModal && (
                <div className="modal show d-block" style={{ background:'rgba(0,0,0,0.5)' }}>
                    <div className="modal-dialog">
                        <div className="modal-content border-0" style={{ borderRadius:12 }}>
                            <div className={`modal-header text-white bg-${stockType === 'in' ? 'success' : 'warning'}`}
                                 style={{ borderRadius:'12px 12px 0 0' }}>
                                <h5 className="modal-title">
                                    <i className={`bi bi-${stockType === 'in' ? 'plus' : 'dash'}-lg me-2`}></i>
                                    Stock {stockType === 'in' ? 'IN' : 'OUT'} — {selectedProduct?.name}
                                </h5>
                                <button className="btn-close btn-close-white"
                                        onClick={() => setShowStockModal(false)}></button>
                            </div>
                            <form onSubmit={handleStockSubmit}>
                                <div className="modal-body">
                                    <div className="mb-2">
                                        <label className="form-label fw-semibold">Quantity</label>
                                        <input name="quantity" type="number" className="form-control"
                                               value={stockForm.quantity} onChange={handleStockChange}
                                               min="1" required />
                                    </div>
                                    <div className="mb-2">
                                        <label className="form-label fw-semibold">Reference No</label>
                                        <input name="referenceNo" className="form-control"
                                               value={stockForm.referenceNo} onChange={handleStockChange}
                                               placeholder="e.g. PO-2026-001" />
                                    </div>
                                    <div className="mb-2">
                                        <label className="form-label fw-semibold">Remarks</label>
                                        <input name="remarks" className="form-control"
                                               value={stockForm.remarks} onChange={handleStockChange} />
                                    </div>
                                </div>
                                <div className="modal-footer">
                                    <button type="button" className="btn btn-secondary"
                                            onClick={() => setShowStockModal(false)}>Cancel</button>
                                    <button type="submit"
                                            className={`btn btn-${stockType === 'in' ? 'success' : 'warning'}`}>
                                        <i className="bi bi-check-lg me-1"></i>Confirm
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
