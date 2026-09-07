import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Banner`;

const authHeader = () => {
    const token = localStorage.getItem("token");

    return {
        Authorization: `Bearer ${token}`
    };
};

export const getBanners = async () => {
    const response = await axios.get(API_URL, {
        headers: authHeader()
    });

    return response.data;
};

export const getBanner = async (id) => {
    const response = await axios.get(`${API_URL}/${id}`, {
        headers: authHeader()
    });

    return response.data;
};

export const addBanner = async (banner) => {
    const response = await axios.post(
        API_URL,
        banner,
        {
            headers: authHeader()
        }
    );

    return response.data;
};

export const updateBanner = async (id, banner) => {
    const response = await axios.put(
        `${API_URL}/${id}`,
        banner,
        {
            headers: authHeader()
        }
    );

    return response.data;
};

export const deleteBanner = async (id) => {
    const response = await axios.delete(
        `${API_URL}/${id}`,
        {
            headers: authHeader()
        }
    );

    return response.data;
};