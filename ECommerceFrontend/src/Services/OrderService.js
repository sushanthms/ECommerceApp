import api from "./api";

export const getOrdersPaged = async (page = 1, pageSize = 20) => {
    const response = await api.get(`/Order/admin?page=${page}&pageSize=${pageSize}`);

    return response.data;
};

export const getOrderById = async (id) => {
    const response = await api.get(`/Order/admin/${id}`);

    return response.data;
};

export const getOrderStats = async () => {
    const response = await api.get("/Order/admin/stats");

    return response.data;
};

export const updateOrderStatus = async (id, status) => {
    const response = await api.put(`/Order/admin/${id}/status`, JSON.stringify(status),
    {
        headers: { "Content-Type": "application/json" }
    }
);

    return response.data;
};

export const cancelOrder = async (id) => {
    const response = await api.put(`/Order/User/${id}/cancel`);

    return response.data;
};