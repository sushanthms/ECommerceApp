import { createContext, useContext, useState, useRef, useEffect} from "react";
import "./Toast.css";

const ToastContext = createContext(null);

export function ToastProvider({ children }) {
  const [toast, setToast] = useState(null);
  const timerRef = useRef(null);

  const showToast = (message, type = "info") => {
    setToast({ message, type });

    clearTimeout(timerRef.current);

    timerRef.current = setTimeout(() => {
      setToast(null);
    }, 3000);
  };

  useEffect(() => {
    return () => clearTimeout(timerRef.current);
  }, []);

  return (
    <ToastContext.Provider value={showToast}>
      {toast && (
        <div className={`toast toast-${toast.type}`} role="status">
          <span>{toast.message}</span>
          <button type="button" onClick={() => setToast(null)} aria-label="Close notification">×</button>
        </div>
      )}
      {children}
    </ToastContext.Provider>
  );
}

export function useToast() {
  return useContext(ToastContext);
}