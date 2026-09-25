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
});
