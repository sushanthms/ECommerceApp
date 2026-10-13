import { createContext, useContext, useState, useEffect } from "react";

const ViewContext = createContext();

export function ViewProvider({ children }) {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(() => {
        return localStorage.getItem("theme") === "dark";// saves true or false in the darkmode useState in react, in localstorage it saves theme:dark or light 
    });

    useEffect(() => {
        const theme = darkMode ? "dark" : "light";

        localStorage.setItem("theme", theme);
        document.documentElement.setAttribute("data-theme", theme);
    }, [darkMode]);

    return (// shares the values with the components inside it without passing them through props
        <ViewContext.Provider value={{menuOpen, setMenuOpen, darkMode, setDarkMode}}>
            {children}
        </ViewContext.Provider>
    );
}

export function useView() {
    return useContext(ViewContext);
}