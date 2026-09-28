import api from './axiosConfig';

describe('axiosConfig', () => {
  afterEach(() => {
    localStorage.clear();
  });

  test('attaches Authorization header if token exists in localStorage', () => {
    localStorage.setItem('token', 'fake-jwt-token');
    
    // Simulate interceptor logic
    const config = { headers: {} };
    // Get the first fulfilled request interceptor
    const requestInterceptor = api.interceptors.request.handlers[0].fulfilled;
    const newConfig = requestInterceptor(config);

    expect(newConfig.headers.Authorization).toBe('Bearer fake-jwt-token');
  });

  test('does not attach Authorization header if token does not exist', () => {
    const config = { headers: {} };
    const requestInterceptor = api.interceptors.request.handlers[0].fulfilled;
    const newConfig = requestInterceptor(config);

    expect(newConfig.headers.Authorization).toBeUndefined();
  });

  describe('response interceptor', () => {
    let originalWindowLocation;
    let originalConsoleError;
    let originalAlert;
    let responseErrorInterceptor;

    beforeEach(() => {
      originalWindowLocation = window.location;
      delete window.location;
      window.location = { href: '' };

      originalConsoleError = console.error;
      console.error = vi.fn();

      originalAlert = window.alert;
      window.alert = vi.fn();

      // Get the rejected response interceptor
      responseErrorInterceptor = api.interceptors.response.handlers[0].rejected;
    });

    afterEach(() => {
      window.location = originalWindowLocation;
      console.error = originalConsoleError;
      window.alert = originalAlert;
    });

    test('handles 401 Unauthorized', async () => {
      localStorage.setItem('token', 'fake-token');
      localStorage.setItem('role', 'Student');
      const error = { response: { status: 401 } };
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error);
      
      expect(localStorage.getItem('token')).toBeNull();
      expect(localStorage.getItem('role')).toBeNull();
      expect(window.location.href).toBe('/login');
    });

    test('handles 403 Forbidden', async () => {
      const error = { response: { status: 403 } };
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error);
      
      expect(console.error).toHaveBeenCalledWith(expect.stringContaining("403 Forbidden"));
      expect(window.alert).toHaveBeenCalledWith(expect.stringContaining("You do not have permission"));
    });

    test('handles 400 Bad Request', async () => {
      const error = { response: { status: 400, data: 'Invalid data' } };
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error);
      
      expect(console.error).toHaveBeenCalledWith(expect.stringContaining("400 Bad Request"), 'Invalid data');
    });

    test('handles 500 Internal Server Error', async () => {
      const error = { response: { status: 500 } };
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error);
      
      expect(console.error).toHaveBeenCalledWith(expect.stringContaining("500 Internal Server Error"));
      expect(window.alert).toHaveBeenCalledWith(expect.stringContaining("unexpected error occurred on the server"));
    });

    test('handles unknown or network errors', async () => {
      const error = { message: 'Network Error' };
      
      await expect(responseErrorInterceptor(error)).rejects.toEqual(error);
      
      expect(console.error).toHaveBeenCalledWith(expect.stringContaining("Network or unknown error"), 'Network Error');
    });
  });
});
