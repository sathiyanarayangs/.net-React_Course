import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5238/api', // Points to Week 6 Auth API
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
}, (error) => {
  return Promise.reject(error);
});
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const config = error.config;
    
    // Task 6.10 Resilience: Retry up to 2 times for network errors or 5xx server errors
    if ((!error.response || error.response.status >= 500) && config) {
      config._retryCount = config._retryCount || 0;
      if (config._retryCount < 2) {
        config._retryCount += 1;
        console.warn(`Network/Server error detected. Retrying request (Attempt ${config._retryCount} of 2)...`);
        
        // Wait before retrying (exponential backoff: 1s, 2s)
        await new Promise(resolve => setTimeout(resolve, 1000 * config._retryCount));
        return api(config);
      }
    }

    if (error.response) {
      const status = error.response.status;
      if (status === 401) {
        // Unauthorized -> Clear token and re-login
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        
        // Prevent infinite reload loop if already on login page
        // This allows LoginPage.jsx to catch the error and display "Invalid credentials"
        if (window.location.pathname !== '/login') {
          window.location.href = '/login';
        }
      } else if (status === 403) {
        // Forbidden
        console.error("403 Forbidden: You do not have permission to perform this action.");
        alert("You do not have permission to perform this action.");
      } else if (status === 400) {
        // Bad Request
        console.error("400 Bad Request: ", error.response.data);
      } else if (status === 500) {
        // Internal Server Error (after retries exhausted)
        console.error("500 Internal Server Error: The server encountered a problem.");
        alert("An unexpected error occurred on the server. Please try again later.");
      }
    } else {
      console.error("Network or unknown error: ", error.message);
    }
    return Promise.reject(error);
  }
);

export default api;
