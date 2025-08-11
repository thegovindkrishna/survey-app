# Authentication Guards Documentation

## Overview
This application uses function-based Angular guards to enforce authentication and authorization throughout the application.

## Available Guards

### 1. `authGuard`
- **Purpose**: Ensures user is authenticated before accessing any protected route
- **Behavior**: 
  - If authenticated: Allows access
  - If not authenticated: Redirects to login page with return URL

### 2. `adminGuard` 
- **Purpose**: Ensures user is authenticated AND has admin role
- **Behavior**:
  - If authenticated + admin role: Allows access
  - If not authenticated: Redirects to login page
  - If authenticated but not admin: Redirects to unauthorized page

### 3. `userGuard`
- **Purpose**: Ensures user is authenticated AND has user role
- **Behavior**:
  - If authenticated + user role: Allows access
  - If not authenticated: Redirects to login page
  - If authenticated but not user: Redirects to unauthorized page

### 4. `guestGuard`
- **Purpose**: Prevents authenticated users from accessing auth pages (login/register)
- **Behavior**:
  - If not authenticated: Allows access
  - If authenticated: Redirects to appropriate dashboard based on role

### 5. `adminOnlyGuard` (Enhanced)
- **Purpose**: Strict admin-only access with enhanced error messages
- **Behavior**: Similar to adminGuard but with better user feedback

### 6. `userOnlyGuard` (Enhanced)
- **Purpose**: Strict user-only access with enhanced error messages
- **Behavior**: Similar to userGuard but with better user feedback

## Implementation

### Route Protection
Guards are applied at multiple levels:

#### Main App Routes (`app-routing-module.ts`)
```typescript
{
  path: 'admin',
  canActivate: [adminGuard],
  loadChildren: () => import('./modules/admin/admin-module').then(m => m.AdminModule)
},
{
  path: 'user', 
  canActivate: [userGuard],
  loadChildren: () => import('./modules/user/user-module').then(m => m.UserModule)
}
```

#### Module-level Routes
Additional guards are applied within admin and user modules for enhanced security.

### Error Pages

#### Unauthorized Access (`/unauthorized`)
- Displays when user doesn't have required permissions
- Shows current user role
- Provides navigation options based on user's actual permissions

#### Not Found (`/404`)
- Displays for invalid routes
- Provides navigation back to appropriate dashboard

## User Roles

### Admin Role
- Can access `/admin` routes
- Full access to admin dashboard, survey management, user management
- Redirected to admin dashboard after login

### User Role  
- Can access `/user` routes
- Access to user dashboard, survey participation
- Redirected to user dashboard after login

## Security Features

1. **Token Validation**: Guards check for valid JWT tokens
2. **Role Verification**: Ensures user has correct role for route
3. **Return URL Handling**: Preserves intended destination after login
4. **Double Protection**: Guards applied at both app and module level
5. **User Feedback**: Toast notifications inform users of access issues
6. **Graceful Fallbacks**: Proper error pages for all scenarios

## Testing Guards

To test the guards:

1. **As unauthenticated user**: Try accessing `/admin` or `/user` - should redirect to login
2. **As admin**: Try accessing `/user` - should redirect to unauthorized page  
3. **As user**: Try accessing `/admin` - should redirect to unauthorized page
4. **As authenticated user**: Try accessing `/auth/login` - should redirect to dashboard

## Usage in Components

Components can check user authentication/role status:

```typescript
constructor(private authService: AuthService) {}

// Check if user is authenticated
if (this.authService.isAuthenticated()) {
  // User is logged in
}

// Check user role
if (this.authService.isAdmin()) {
  // User is admin
}

if (this.authService.isUser()) {
  // User is regular user  
}
```

## Configuration

Guards are automatically injected using Angular's dependency injection:
- `AuthService`: Provides authentication state and methods
- `Router`: Handles navigation redirects
- `ToastService`: Shows user-friendly messages

No additional configuration is required - guards work automatically when applied to routes.
