import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Auth`;

export const registerUser = async (userData) => {
    const response = await axios.post(
        `${API_URL}/register`,
        userData
    );

    return response.data;
};

export const loginUser = async (loginData) => {
    const response = await axios.post(
        `${API_URL}/login`,
        loginData
    );

    return response.data;
};