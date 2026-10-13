import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getOrdersPaged } from "../Services/OrderService";

import "./AdminOrders.css";

function AdminOrders() {

    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const pageSize = 20;

    const navigate = useNavigate();

        useEffect(() => {
            const loadOrders = async () => {
                setLoading(true);
                try {
                    const data = await getOrdersPaged(page, pageSize);
                    setOrders(data.orders);
                    setTotalPages(data.totalPages);
                } catch (error) {
                    console.error("Error loading orders:", error);
                } finally {
                    setLoading(false);
                }
            };

            loadOrders();
        }, [page]);

    return (
        <>
            <Header />

            <div className="page-layout">

                <Sidebar />

                <main className="admin-orders-page">

                    <button className="back-btn" onClick={() => navigate("/admin")}>← Back to Home</button>

                    <h1>Orders</h1>

                    {loading ? (

                        <p>Loading orders...</p>

                    ) : orders.length === 0 ? (

                        <p>No orders found.</p>

                    ) : (

                        <div className="orders-table-container">

                            <table className="orders-table">

                                <thead>
                                    <tr>
                                        <th>Order ID</th>
                                        <th>Customer</th>
                                        <th>Date</th>
                                        <th>Total</th>
                                        <th>Payment</th>
                                        <th>Payment Status</th>
                                        <th>Status</th>
                                        <th>Action</th>
                                    </tr>
                                </thead>

                                <tbody>

                                    {orders.map((order) => (

                                        <tr key={order.id}>
                                            <td>#{order.id}</td>
                                            <td>{order.deliveryName}</td>
                                            <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                                            <td>₹{order.totalAmount}</td>
                                            <td>{order.paymentMethod}</td>
                                            <td>{order.paymentStatus}</td>
                                            <td>{order.status}</td>
                                            <td><button onClick={() => navigate(`/admin/orders/${order.id}`)}>View</button></td>
                                        </tr>

                                    ))}

                                </tbody>

                            </table>

                            <div className="pagination">
                                <button onClick={() => setPage(page - 1)} disabled={page === 1}>← Previous</button>
                                <span>Page {page} of {totalPages}</span>
                                <button onClick={() => setPage(page + 1)} disabled={page >= totalPages}>Next →</button>
                            </div>

                        </div>

                    )}

                </main>

            </div>
        </>
    );
}

export default AdminOrders;