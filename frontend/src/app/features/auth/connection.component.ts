import { Component, inject, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-connection',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-slate-50 p-4">
      <mat-card class="w-full max-w-md p-6 bg-white rounded-lg shadow-md">
        <mat-card-header class="mb-6">
          <mat-card-title class="text-2xl font-bold text-slate-900">Connect to NetFormsManager</mat-card-title>
          <mat-card-subtitle class="text-slate-500">Enter your API Token to continue</mat-card-subtitle>
        </mat-card-header>
        
        <mat-card-content>
          <form (ngSubmit)="connect()" class="flex flex-col gap-4">
            <mat-form-field appearance="outline" class="w-full">
              <mat-label>Backend URL</mat-label>
              <input matInput [(ngModel)]="url" name="url" placeholder="http://localhost:8080">
            </mat-form-field>

            <mat-form-field appearance="outline" class="w-full">
              <mat-label>API Token</mat-label>
              <input matInput [(ngModel)]="token" name="token" type="password" required>
            </mat-form-field>

            <mat-checkbox [(ngModel)]="rememberMe" name="rememberMe" color="primary">
              Remember me
            </mat-checkbox>

            <button mat-flat-button color="primary" type="submit" [disabled]="!token()" class="w-full h-12 text-lg">
              Connect
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class ConnectionComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  url = model('http://localhost:8080');
  token = model('');
  rememberMe = model(false);

  constructor() {
    // Pre-fill if available
    this.url.set(this.authService.baseUrl());
  }

  connect() {
    if (this.token()) {
      this.authService.connect(this.token(), this.url(), this.rememberMe());
      this.router.navigate(['/dashboard']);
    }
  }
}
