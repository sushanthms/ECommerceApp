import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Order`;

export const getAllOrders = async () => {
    const token = localStorage.getItem("token");

    const response = await axios.get(API_URL, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
};

export const updateOrderStatus = async (id, status) => {
    const token = localStorage.getItem("token");

    const response = await axios.put(
        `${API_URL}/admin/${id}/status`,
        status,
        {
            headers: {
                Authorization: `Bearer ${token}`,
                "Content-Type": "application/json"
            }
        }
    );

    return response.data;
};

export const cancelOrder = async (id) => {
    const token = localStorage.getItem("token");

    const response = await axios.put(
        `${API_URL}/User/${id}/cancel`,
        {},
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

    return response.data;
};