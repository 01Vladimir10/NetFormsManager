import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormsService } from '../../core/api/forms.service';
import { FormRequestDto } from '../../core/api/models';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        MatButtonModule,
        MatTableModule,
        MatIconModule,
        MatDialogModule
    ],
    template: `
    <div class="container mx-auto p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-3xl font-bold text-slate-900">Forms</h1>
        <button mat-flat-button color="primary" (click)="createForm()">
          <mat-icon>add</mat-icon>
          Create Form
        </button>
      </div>

      <div class="bg-white rounded-lg shadow overflow-hidden">
        <table mat-table [dataSource]="forms()" class="w-full">
          <!-- Name Column -->
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef> Name </th>
            <td mat-cell *matCellDef="let form"> {{form.name}} </td>
          </ng-container>

          <!-- Actions Column -->
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef> Actions </th>
            <td mat-cell *matCellDef="let form">
              <a mat-button color="primary" [routerLink]="['/forms', form.id]">
                View
              </a>
              <button mat-button color="warn" (click)="deleteForm(form)">
                Delete
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>

        <div *ngIf="forms().length === 0" class="p-6 text-center text-slate-500">
          No forms found. Create one to get started.
        </div>
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
    private formsService = inject(FormsService);

    forms = signal<any[]>([]);
    displayedColumns = ['name', 'actions'];

    ngOnInit() {
        this.loadForms();
    }

    loadForms() {
        this.formsService.getForms().subscribe({
            next: (data) => this.forms.set(data),
            error: (err) => console.error('Failed to load forms', err)
        });
    }

    createForm() {
        // For simplicity, just creating a default form or prompting for name could be better.
        // I'll implement a simple prompt or just create a default one for now to keep it simple as per "Create Form" button.
        // Or better, navigate to a "new" form page or open a dialog.
        // Let's create a default one "New Form" and then let user edit it.
        const newForm: FormRequestDto = {
            name: 'New Form',
            fields: [],
            allowedOrigins: []
        };

        this.formsService.createForm(newForm).subscribe({
            next: () => this.loadForms(),
            error: (err) => console.error('Failed to create form', err)
        });
    }

    deleteForm(form: any) {
        if (confirm(`Are you sure you want to delete "${form.name}"?`)) {
            this.formsService.deleteForm(form.id).subscribe({
                next: () => this.loadForms(),
                error: (err) => console.error('Failed to delete form', err)
            });
        }
    }
}
