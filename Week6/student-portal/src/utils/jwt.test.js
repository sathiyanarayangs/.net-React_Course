import { parseJwt } from './jwt';

describe('jwt utility', () => {
  test('parses valid jwt token', () => {
    // A mock token with a simple payload { "role": "Teacher" }
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({ role: 'Teacher' }));
    const signature = 'signature';
    const token = `${header}.${payload}.${signature}`;

    const result = parseJwt(token);
    expect(result).toEqual({ role: 'Teacher' });
  });

  test('returns null for invalid token', () => {
    const result = parseJwt('invalid-token');
    expect(result).toBeNull();
  });
});
