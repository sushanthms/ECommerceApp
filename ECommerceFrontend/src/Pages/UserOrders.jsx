import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import axios from "axios";
import { cancelOrder } from "../Services/OrderService.jsx";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import "./UserOrders.css";

function UserOrders() {

    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [orders, setOrders] = useState([]);
    const [loading, setLoading] = useState(true);

const handleCancelOrder = async (id) => {

    const confirmCancel = window.confirm("Are you sure you want to cancel this order?");

    if (!confirmCancel) {
        return;
    }

    try {

        await cancelOrder(id);

        setOrders((previousOrders) =>
            previousOrders.map((order) =>
                order.id === id
                    ? { ...order, status: "Cancelled" }
                    : order
            )
        );

        alert("Order cancelled successfully.");

    } catch (error) {

        console.error("Error cancelling order:", error);
        alert(error.response?.data?.message || "Failed to cancel order.");
    }
};

    useEffect(() => {
        const fetchOrders = async () => {
            try {

             // const result = window.confirm("Continue?");
             // console.log(result);
                const token = localStorage.getItem("token");

                const response = await axios.get(
                    `${import.meta.env.VITE_API_URL}/Order/UserOrders`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`
                        }
                    }
                );

                setOrders(response.data);
            } catch (error) {
                console.error("Error fetching orders:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchOrders();
    }, []);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")} />

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role="User" />

                <div className="orders-page">

                    {loading ? (
                        <p>Loading orders...</p>
                    ) : (
                        <>
                            <h2>My Orders</h2>

                            {orders.length === 0 ? (
                                <div className="no-orders">
                                    <h3>No orders yet</h3>
                                    <p>You have not placed any orders yet.</p>
                                </div>
                            ) : (
                                <div className="orders-list">

                                    {orders.map((order) => {

                                        const orderItems = order.orderItems || [];
                                        const visibleItems = orderItems.slice(0, 3);
                                        const remainingItems = orderItems.length - 3;

                                        return (
                                            <div className="order-card" key={order.id}>

                                                <div className="order-header">

                                                    <div>
                                                        <h3>Order #{order.id}</h3>
                                                        <p>Ordered on{" "}{new Date(order.orderDate).toLocaleDateString()}</p>
                                                    </div>
                                                    <span className={`order-status ${order.status.toLowerCase()}`}>{order.status}</span>
                                                </div>

                                                {visibleItems.length > 0 && (
                                                    <div className="order-products">

                                                        {visibleItems.map((item) => (

                                                            <div className="order-product" key={item.id}>

                                                                {item.productImage ? (
                                                                    <img src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${item.productImage}`} alt={item.productName} className="order-product-image"
                                                                    />
                                                                ) : (
                                                                    <div className="order-product-fallback">📦</div>
                                                                )}

                                                                <div className="order-product-info">
                                                                    <h3>{item.productName}</h3>
                                                                    <p>Quantity: {item.quantity}</p>
                                                                    <p>₹{item.price}</p>
                                                                </div>
                                                            </div>

                                                        ))}

                                                        {remainingItems > 0 && (
                                                            <p className="more-items">+ {remainingItems} more product{remainingItems !== 1 ? "s" : ""}</p>
                                                        )}

                                                    </div>
                                                )}

                                                <div className="order-info">
                                                    <p>{orderItems.length} product{orderItems.length !== 1 ? "s" : ""}</p>
                                                    <p>Total: <strong>₹{order.totalAmount}</strong></p>
                                                    <p>Payment Method: {order.paymentMethod}</p>
                                                    <p>Payment Status: {order.paymentStatus}</p>
                                                </div>

                                                <div className="order-footer">
                                                    <Link to={`/orders/${order.id}`} className="view-order-button">View Order Details</Link>
                                                    {(order.status === "Pending" || order.status === "Processing") && (
                                                        <button className="cancel-order-button" onClick={() => handleCancelOrder(order.id)}>Cancel Order</button>
                                                    )}

                                                </div>

                                            </div>
                                        );
                                    })}

                                </div>
                            )}

                        </>
                    )}

                </div>

            </div>
        </>
    );
}

export default UserOrders;