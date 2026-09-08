import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Product`;

export const getProducts = async () => {
    const response = await axios.get(API_URL);
    return response.data;
};

export const getAdminProducts = async () => {
    const token = localStorage.getItem("token");

    const response = await axios.get(`${API_URL}/admin`, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });

    return response.data;
};

export const addProduct = async (product, token) => {
    const response = await axios.post(API_URL, product, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });
    return response.data;
};

export const updateProduct = async (id, product, token) => {
    const response = await axios.put(`${API_URL}/${id}`, product, {
        headers: {
            Authorization: `Bearer ${token}`
        }
    });
    return response.data;
};

export const searchProducts = async (search) => {
    const response = await axios.get(
        `${API_URL}/search?search=${encodeURIComponent(search)}`
    );

    return response.data;
};

export const searchAdminProducts = async (search) => {
    const token = localStorage.getItem("token");

    const response = await axios.get(
        `${API_URL}/admin/search?search=${encodeURIComponent(search)}`,
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

    return response.data;
};