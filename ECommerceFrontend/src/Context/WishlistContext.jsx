import { createContext, useContext, useEffect, useState } from "react";
import { useAuth } from "./AuthContext.jsx";
import { useToast } from "./ToastContext.jsx";
import { getWishlistIds, addToWishlist, removeFromWishlist } from "../Services/WishlistService";

const WishlistContext = createContext();

export function WishlistProvider({ children }) {
    const { user, isLoggedIn } = useAuth();
    const showToast = useToast();

    const [ids, setIds] = useState([]);

    const isUser = isLoggedIn && user?.role === "User";

    useEffect(() => {
        if (!isUser) {
            setIds([]);
            return;
        }
// .then(setIds) .then calls setIds(ids) when the data arrives        
// .then(setIds()) setIds() runs immediately with undefined, and .then receives its return value (nothing).
        getWishlistIds()
            .then(setIds)// after getWishlistIds finishes it calls setIds 
            .catch((error) => console.error("Error loading wishlist:", error));
    }, [isUser, user?.userId]);

    const isWishlisted = (productId) => ids.includes(productId);

    const toggleWishlist = async (productId) => {
        const wasSaved = ids.includes(productId);

        // update the screen first, so the heart reacts instantly
        // is wasSaved is true, filter builds a new array keeping every id that is not productId
        setIds((prev) => wasSaved ? prev.filter((id) => id !== productId) : [...prev, productId]);

        try {
            if (wasSaved) {
                await removeFromWishlist(productId);
                showToast("Removed from Wishlist", "error");
            } else {
                await addToWishlist(productId);
                showToast("Added to Wishlist", "Success");
            }
        } catch (error) {
            // the server refused, so put the heart back
            setIds((prev) => wasSaved ? [...prev, productId] : prev.filter((id) => id !== productId));
            showToast(error.response?.data?.message || "Could not update wishlist.", "error");
        }
    };

    const forgetProduct = (productId) => {
        setIds((prev) => prev.filter((id) => id !== productId));
    };

    return (
        <WishlistContext.Provider value={{ ids, isWishlisted, toggleWishlist, forgetProduct, isUser }}>
            {children}
        </WishlistContext.Provider>
    );
}

export const useWishlist = () => useContext(WishlistContext);