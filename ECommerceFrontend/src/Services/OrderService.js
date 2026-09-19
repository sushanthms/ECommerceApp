import api from "./api";

export const getAllOrders = async () => {
    const response = await api.get("/Order");

    return response.data;
};

export const updateOrderStatus = async (id, status) => {
    const response = await api.put(
        `/Order/admin/${id}/status`,
        status
    );

    return response.data;
};

export const cancelOrder = async (id) => {
    const response = await api.put(
        `/Order/User/${id}/cancel`,
        {}
    );

    return response.data;
};