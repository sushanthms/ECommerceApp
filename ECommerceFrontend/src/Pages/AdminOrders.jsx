import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { getAllOrders } from "../Services/OrderService.jsx";

import "./AdminOrders.css";

function AdminOrders() {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);

    const navigate = useNavigate();

    useEffect(() => {

        const loadOrders = async () => {

            try {

                const data = await getAllOrders();

                setOrders(data);

            } catch (error) {

                console.error("Error loading orders:", error);

            } finally {

                setLoading(false);

            }
        };

        loadOrders();

    }, []);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode}role="Admin"/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} role="Admin" />

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
                                            <td>{order.status}</td>
                                            <td><button onClick={() =>navigate(`/admin/orders/${order.id}`,{state: {order}})}>View</button></td>
                                        </tr>// state order means sending order object along with the navigation

                                    ))}

                                </tbody>

                            </table>

                        </div>

                    )}

                </main>

            </div>
        </>
    );
}

export default AdminOrders;