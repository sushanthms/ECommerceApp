import { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getOrderById, updateOrderStatus } from "../Services/OrderService";

import "./AdminOrderDetails.css";

function AdminOrderDetails() {

    const location = useLocation();
    const navigate = useNavigate();
    const { id } = useParams();

    const [updatingStatus, setUpdatingStatus] = useState(false);
    const [order, setOrder] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadOrder = async () => {
            setLoading(true);
            try {
                const data = await getOrderById(id);
                setOrder(data);
            } catch (error) {
                console.error("Error loading order:", error);
                setOrder(null);
            } finally {
                setLoading(false);
            }
        };

        loadOrder();
    }, [id]);

    if (loading) {
        return (<p>Loading order...</p>);
    }

    if (!order) {
        return (<p>Order not found.</p>);
    }

const handleStatusChange = async (e) => {

    const newStatus = e.target.value;

    try {

        setUpdatingStatus(true);

        await updateOrderStatus(order.id, newStatus);

        setOrder({...order, status: newStatus});

    } catch (error) {

        console.error("Error updating order status:", error);
        alert(error.response?.data?.message || "Failed to update order status.");

    } finally {

        setUpdatingStatus(false);

    }
};

    return (
        <>
            <Header />

            <div className="page-layout">

                <Sidebar />
                <main className="admin-orders-page">

                    <button className="back-btn" onClick={() =>navigate("/admin/orders")}>← Back to Orders</button>

                    <h1>Order #{order.id}</h1>

                    <div className="order-detail-section">
                        <h3>Order Details</h3>
                        <p><strong>Date:</strong>{" "}{new Date(order.orderDate).toLocaleString()}</p>
                        <p>
                            <strong>Status:</strong>{" "}
                            {order.status === "Cancelled" ?
                            ( 
                            <span>Cancelled</span> ) 
                            : (
                            <select value={order.status} onChange={handleStatusChange} disabled={updatingStatus}>
                                <option value="Pending">Pending</option>
                                <option value="Processing">Processing</option>
                                <option value="Shipped">Shipped</option>
                                <option value="Delivered">Delivered</option>
                                <option value="Cancelled">Cancelled</option>
                            </select>
                            )}
                        </p>
                        <p><strong>Payment Method:</strong>{" "}{order.paymentMethod}</p>
                        <p><strong>Payment Status:</strong>{" "}{order.paymentStatus}</p>
                    </div>
                    <div className="order-detail-section">
                        <h3>Delivery Address</h3>
                        <p><strong>Name:</strong>{" "}{order.deliveryName}</p>
                        <p><strong>Address:</strong>{" "}{order.address}</p>
                        <p><strong>City:</strong>{" "}{order.city}</p>
                        <p><strong>State:</strong>{" "}{order.state}</p>
                        <p><strong>Pincode:</strong>{" "}{order.pincode}</p>
                    </div>
                    <div className="order-detail-section">
                        <h3>Order Details</h3>
                        <p><strong>Date:</strong>{" "}{new Date(order.orderDate).toLocaleString()}</p>
                        <p><strong>Status:</strong>{" "}{order.status}</p>
                    </div>
                    <div className="order-detail-section">
                        <h3>Ordered Products</h3>
                        <table className="order-items-table">
                            <thead>
                                <tr>
                                    <th>Product</th>
                                    <th>Price</th>
                                    <th>Quantity</th>
                                    <th>Subtotal</th>
                                </tr>

                            </thead>

                            <tbody>

                                {order.orderItems.map((item) => (

                                    <tr key={item.id}>
                                        <td>{item.productName}</td>
                                        <td>₹{item.price}</td>
                                        <td>{item.quantity}</td>
                                        <td>₹{(item.price * item.quantity).toFixed(2)}</td>
                                    </tr>

                                ))}

                            </tbody>

                        </table>

                    </div>

                    <div className="order-total">
                        <strong>Total Amount: ₹{order.totalAmount}</strong>
                    </div>

                </main>

            </div>
        </>
    );
}

export default AdminOrderDetails;