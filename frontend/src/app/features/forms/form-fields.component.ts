import { Component, inject, signal, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsService } from '../../core/api/forms.service';
import { FormField } from '../../core/api/models';

@Component({
    selector: 'app-form-fields',
    standalone: true,
    imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule],
    template: `
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-xl font-semibold text-slate-800">Form Fields</h2>
      <button mat-stroked-button color="primary">
        <mat-icon>add</mat-icon>
        Add Field
      </button>
    </div>

    <table mat-table [dataSource]="fields()" class="w-full border border-slate-200 rounded-lg overflow-hidden">
      <ng-container matColumnDef="name">
        <th mat-header-cell *matHeaderCellDef> Name </th>
        <td mat-cell *matCellDef="let field"> {{field.name}} </td>
      </ng-container>

      <ng-container matColumnDef="type">
        <th mat-header-cell *matHeaderCellDef> Type </th>
        <td mat-cell *matCellDef="let field"> {{field.type}} </td>
      </ng-container>

      <ng-container matColumnDef="required">
        <th mat-header-cell *matHeaderCellDef> Required </th>
        <td mat-cell *matCellDef="let field"> 
          <mat-icon *ngIf="field.isRequired" class="text-green-500">check_circle</mat-icon>
        </td>
      </ng-container>

      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef> Actions </th>
        <td mat-cell *matCellDef="let field">
          <button mat-icon-button color="warn">
            <mat-icon>delete</mat-icon>
          </button>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>

    <div *ngIf="fields().length === 0" class="p-4 text-center text-slate-500 bg-slate-50 rounded-lg mt-2">
      No fields defined.
    </div>
  `
})
export class FormFieldsComponent implements OnChanges {
    @Input() formId!: string;
    private formsService = inject(FormsService);

    fields = signal<FormField[]>([]);
    displayedColumns = ['name', 'type', 'required', 'actions'];

    ngOnChanges(changes: SimpleChanges) {
        if (changes['formId'] && this.formId) {
            this.loadFields();
        }
    }

    loadFields() {
        this.formsService.getFormFields(this.formId).subscribe({
            next: (data) => this.fields.set(data),
            error: (err) => console.error('Failed to load fields', err)
        });
    }
}
