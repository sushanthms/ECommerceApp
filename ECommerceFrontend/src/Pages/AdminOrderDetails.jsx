import { useEffect, useState } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
// useLocation is used to access the state that was passed during navigation.
// useParams is used to get the parameters from the URL. we have id in the url
import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { getAllOrders } from "../Services/OrderService.jsx";

import "./AdminOrderDetails.css";
// when id changes the url becomes /admin/orders/:id. when ever this url is updated means new id is being searched,
// the url first goes to App.jsx then App.jsx renders AdminOrderDetails through ProtectedRoutes.jsx.
// now AdminOrderDetails.jsx runs from top to bottom for every render means for every id in the url. so this function runs
// App.jsx does not necessarily render AdminOrderDetails from scratch every time the ID changes.
function AdminOrderDetails() {

    const location = useLocation();
    const navigate = useNavigate();
    const { id } = useParams();

    const [order, setOrder] = useState(location.state?.order || null);// order object
    const [loading, setLoading] = useState(!location.state?.order);// if order was not passed then loading is true and shows Loading.
    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");
// even if the id is updated by clicking view orders or by directly typing admin/orders/7 the process in AdminOrderDetails is same
// if we click view order, the order object will be passed to the AdminOrderDetails page, if we directly type admin/Orders/7 the order object will be fetched from the backend
    useEffect(() => {
        if (order.id === Number(id)) {// order.id is from the stored order state which happened in previous useEffect run
            return;
        }
    setLoading(true);

     const loadOrder = async () => {

            try {

                const data = await getAllOrders();

                const foundOrder = data.find(
                    (item) => item.id === Number(id)
                );

                setOrder(foundOrder || null);

            } catch (error) {
                console.error("Error loading order:", error);

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

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="Admin"/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} role="Admin" />
                <main className="admin-orders-page">

                    <button className="back-btn" onClick={() =>navigate("/admin/orders")}>← Back to Orders</button>

                    <h1>Order #{order.id}</h1>

                    <div className="order-detail-section">

                        <h3>Customer Details</h3>

                        <p><strong>Name:</strong>{" "}{order.customerName}</p>

                        <p><strong>Email:</strong>{" "}{order.customerEmail}</p>

                        <p><strong>Phone:</strong>{" "}{order.phone}</p>

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