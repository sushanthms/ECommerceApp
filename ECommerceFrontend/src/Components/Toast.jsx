import "./Toast.css";

function Toast({ message }) {
    if (!message) return null;

    return (
        <div className="toast">
            {message}
        </div>
    );
}

export default Toast;

/* const [toast, setToast] = useState(""); when first App.jsx runs toast is an empty string here.
so when <Toast message={toast} /> renders it passess <Toast message="" /> to Toast.jsx.
Then Toast.jsx returns null. when userhome calls showToast("Product added!"); it executes App.jsx fucntion.
App.jsx is the parent fucntion so we dont need to import App.jsx.
then App.jsx runs 
const showToast = (message) => {
    setToast(message);
};

now setToast("Product added!"); and toast state stores "Product added!"

we dont need to write useEffect to re-render the screen because React re-renders automatically when a state setter is called.
setToast is a state setter function provided by useState.*/