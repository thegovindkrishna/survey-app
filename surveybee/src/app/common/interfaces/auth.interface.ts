export interface AuthRequest {
  email: string;
  password: string;
  role?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  email: string;
  username: string;
  role: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface User {
  email: string;
  role: string;
}

export interface TokenPayload {
  sub: string; // user id
  email: string;
  role: string;
  exp: number;
  iat: number;
}
