import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { AuthProvider } from "./Context/AuthContext.jsx";
import { ToastProvider } from "./Context/ToastContext.jsx";
import { ViewProvider } from "./Context/ViewContext.jsx";
import { WishlistProvider } from './Context/WishlistContext.jsx';
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AuthProvider>
      <ViewProvider>
        <ToastProvider>
          <WishlistProvider>
            <App />
            </WishlistProvider>
          </ToastProvider>
      </ViewProvider>
    </AuthProvider>
  </StrictMode>,
)