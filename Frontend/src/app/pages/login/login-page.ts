import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';

import { AuthService, LoginRequest, LoginResponse } from '../../_services/auth.service';

@Component({
  selector: 'app-login-page',
  imports: [FormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
})
export class LoginPage implements OnInit {
  email = '';
  password = '';
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal('');

  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/orders']);
    }
  }

  onSubmit(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const payload: LoginRequest = {
      email: this.email,
      password: this.password,
    };

    this.authService
      .login(payload)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (response: LoginResponse) => {
          this.authService.saveToken(response.token);
          this.router.navigate(['/orders']);
        },
        error: () => {
          this.errorMessage.set('Invalid email or password.');
        },
      });
  }
}
