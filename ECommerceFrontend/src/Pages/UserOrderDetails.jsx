import { useEffect, useState } from "react";
import { useParams, Link, useNavigate } from "react-router-dom";
import axios from "axios";
import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import "./UserOrderDetails.css";

function UserOrderDetails() {

    const { id } = useParams();
    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [order, setOrder] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchOrderDetails = async () => {
            try {
                const token = localStorage.getItem("token");

                const response = await axios.get(
                    `${import.meta.env.VITE_API_URL}/Order/User/${id}`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`
                        }
                    }
                );

                setOrder(response.data);
            } catch (error) {
                console.error("Error fetching order details:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchOrderDetails();
    }, [id]);

    if (loading) {
        return (
            <div className="order-details-page">
                <p>Loading order...</p>
            </div>
        );
    }

    if (!order) {
        return (
            <div className="order-details-page">
                <p>Order not found.</p>
            </div>
        );
    }

    const orderItems = order.orderItems || [];

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")} />

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role="User" />

                <div className="order-details-page">

                    <div className="order-top">
                        <div>
                            <h2>Order Details</h2>
                            <p>Order #{order.id} •{" "}{new Date(order.orderDate).toLocaleDateString()}</p>
                        </div>

                        <span className={`order-details-status ${order.status.toLowerCase()}`}>{order.status}</span>
                    </div>

                    <div className="order-details-card">
                        <h3>Items in this order</h3>

                        <div className="order-items">
                            {orderItems.map((item) => (
                                <div className="order-detail-item" key={item.id}>

                                    <div className="order-item-left">

                                        {item.productImage ? (
                                            <img src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${item.productImage}`} alt={item.productName} className="order-detail-image" />
                                        ) : (
                                            <div className="order-detail-image-fallback">📦</div>
                                        )}

                                        <div className="order-item-info">
                                            <h4>{item.productName}</h4>
                                            <p>Quantity: {item.quantity}</p>
                                            <p>Price: ₹{item.price}</p>
                                        </div>

                                    </div>
                                    <strong>₹{Math.round(item.price * item.quantity)}</strong>
                                </div>
                            ))}
                        </div>
                    </div>

                    <div className="order-details-card">
                        <h3>Delivery Address</h3>

                        <div className="order-address">
                            <p className="delivery-name">{order.deliveryName}</p>
                            <p>{order.phone}</p>
                            <p>{order.address}</p>
                            <p>{order.city}, {order.state} - {order.pincode}</p>
                        </div>
                    </div>

                    <div className="order-details-card">
                        <h3>Order Summary</h3>

                        <div className="summary-row">
                            <span>Items</span>
                            <span>{orderItems.length}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Ordered Date</span>
                            <span>{new Date(order.orderDate).toLocaleDateString()}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Order Status</span>
                            <span>{order.status.toLowerCase()}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Payment Method</span>
                            <span>{order.paymentMethod}</span>
                        </div>

                        <div className="summary-row total-row">
                            <span>Payment Status</span>
                            <span>{order.paymentStatus}</span>
                        </div>
                        <div className="summary-row total-row">
                            <span>Total Amount</span>
                            <strong>₹{Math.round(order.totalAmount)}</strong>
                        </div>
                    </div>

                    <Link to="/orders" className="back-orders-button">Back to Orders</Link>

                </div>

            </div>
        </>
    );
}

export default UserOrderDetails;