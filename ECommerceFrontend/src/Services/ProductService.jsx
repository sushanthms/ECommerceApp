import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Product`;

export const getProducts = async (page = 1, pageSize = 20, category = "") => {
    const response = await axios.get(
        `${API_URL}?page=${page}&pageSize=${pageSize}&category=${encodeURIComponent(category)}`
    );

    return response.data;
};

export const getAdminProducts = async (page = 1, pageSize = 20) => {

    const token = localStorage.getItem("token");

    const response = await axios.get(
        `${API_URL}/admin?page=${page}&pageSize=${pageSize}`,
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

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

export const uploadProducts = async (file) => {

    const token = localStorage.getItem("token");

    const formData = new FormData();
    formData.append("file", file);

    const response = await axios.post(
        `${API_URL}/upload`,
        formData,
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

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

export const searchProducts = async (search, page = 1, pageSize = 20,category = "") => {
    const response = await axios.get(
        `${API_URL}/search?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}&category=${encodeURIComponent(category)}`
    );

    return response.data;
};

export const searchAdminProducts = async (search, page = 1, pageSize = 20) => {

    const token = localStorage.getItem("token");

    const response = await axios.get(
        `${API_URL}/admin/search?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`,
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

    return response.data;
};

export const hideProduct = async (id) => {
    const token = localStorage.getItem("token");

    const response = await axios.put(
        `${API_URL}/${id}/hide`,
        {},
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

    return response.data;
};

export const restoreProduct = async (id) => {
    const token = localStorage.getItem("token");

    const response = await axios.put(
        `${API_URL}/${id}/restore`,
        {},
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

    return response.data;
};

export const permanentDeleteProduct = async (id) => {
    const token = localStorage.getItem("token");

    const response = await axios.delete(
        `${API_URL}/${id}/permanent`,
        {
            headers: {
                Authorization: `Bearer ${token}`
            }
        }
    );

    return response.data;
};

export const getCategories = async () => {
    const response = await axios.get(`${API_URL}/categories`);
    return response.data;
};

export const getProductById = async (id) => {
    const response = await axios.get(`${API_URL}/${id}`);
    return response.data;
};