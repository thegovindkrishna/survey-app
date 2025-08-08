import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../common/services/auth.service';
import { ToastService } from '../../../../common/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  private returnUrl: string;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    // Get return url from route parameters or default to appropriate dashboard
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(event?: Event) {
    // Prevent default form submission
    event?.preventDefault();
    
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const { email, password } = this.loginForm.value;

    console.log('=== LOGIN ATTEMPT START ===');
    console.log('Attempting login with:', { email, password });
    console.log('Auth service API base:', this.authService['API_BASE']);
    
    // Store in sessionStorage to persist across page reloads
    sessionStorage.setItem('loginDebug', JSON.stringify({
      timestamp: new Date().toISOString(),
      email: email,
      step: 'login_started'
    }));

    this.authService.login(email, password).subscribe({
      next: (response) => {
        console.log('=== LOGIN SUCCESS ===');
        console.log('SUCCESS: Login response received:', response);
        
        // Store success in sessionStorage
        sessionStorage.setItem('loginDebug', JSON.stringify({
          timestamp: new Date().toISOString(),
          email: email,
          step: 'login_success',
          response: response
        }));
        
        this.isLoading.set(false);
        
        console.log('User role:', response.role);
        console.log('Role type:', typeof response.role);
        console.log('Role exact value:', JSON.stringify(response.role));
        console.log('Email:', response.email);
        console.log('Full response object:', response);
        console.log('Role === "Admin":', response.role === 'Admin');
        console.log('Role === "User":', response.role === 'User');
        console.log('Current auth state:', this.authService.isUserAuthenticated());
        console.log('Is admin:', this.authService.hasAdminRole());

        // Show success toast
        this.toastService.success(`Welcome back, ${response.email}!`, 'Login Successful');

        // Redirect based on user role - should be exactly "Admin" or "User"
        const targetRoute = response.role === 'Admin' ? '/admin/dashboard' : '/user';
        console.log('Target route determined:', targetRoute);
        
        sessionStorage.setItem('loginDebug', JSON.stringify({
          timestamp: new Date().toISOString(),
          email: email,
          step: 'navigation_attempt',
          targetRoute: targetRoute
        }));
        
        // Try different navigation approaches
        this.router.navigate([targetRoute]).then(
          (success) => {
            console.log('Navigation successful:', success);
            sessionStorage.setItem('loginDebug', JSON.stringify({
              timestamp: new Date().toISOString(),
              step: 'navigation_result',
              success: success
            }));
            
            if (!success) {
              console.log('Navigation failed, but NOT using window.location fallback to avoid page reload');
              // Instead, let's try navigateByUrl
              this.router.navigateByUrl(targetRoute).then((urlSuccess) => {
                console.log('NavigateByUrl result:', urlSuccess);
                sessionStorage.setItem('loginDebug', JSON.stringify({
                  timestamp: new Date().toISOString(),
                  step: 'navigateByUrl_result',
                  success: urlSuccess
                }));
              });
            }
          },
          (error) => {
            console.error('Navigation failed:', error);
            sessionStorage.setItem('loginDebug', JSON.stringify({
              timestamp: new Date().toISOString(),
              step: 'navigation_error',
              error: error.toString()
            }));
            console.log('Navigation error, but NOT using window.location fallback');
          }
        );
      },
      error: (error) => {
        console.error('ERROR: Login failed:', error);
        console.error('Full error object:', error);
        this.isLoading.set(false);
        this.errorMessage.set(error.message || 'Login failed. Please try again.');

        // Show error toast
        this.toastService.error(error.message || 'Login failed. Please check your credentials and try again.', 'Login Failed');
      }
    });
  }
}
