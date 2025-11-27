import { Component, inject, signal, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsService } from '../../core/api/forms.service';

@Component({
    selector: 'app-form-templates',
    standalone: true,
    imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatSlideToggleModule],
    template: `
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-xl font-semibold text-slate-800">Email Templates</h2>
      <button mat-stroked-button color="primary">
        <mat-icon>add</mat-icon>
        Add Template
      </button>
    </div>

    <table mat-table [dataSource]="templates()" class="w-full border border-slate-200 rounded-lg overflow-hidden">
      <ng-container matColumnDef="subject">
        <th mat-header-cell *matHeaderCellDef> Subject </th>
        <td mat-cell *matCellDef="let tpl"> {{tpl.subjectTemplate}} </td>
      </ng-container>

      <ng-container matColumnDef="enabled">
        <th mat-header-cell *matHeaderCellDef> Enabled </th>
        <td mat-cell *matCellDef="let tpl">
          <mat-slide-toggle [checked]="tpl.isEnabled" (change)="toggle(tpl, $event.checked)"></mat-slide-toggle>
        </td>
      </ng-container>

      <ng-container matColumnDef="actions">
        <th mat-header-cell *matHeaderCellDef> Actions </th>
        <td mat-cell *matCellDef="let tpl">
          <button mat-icon-button color="primary">
            <mat-icon>edit</mat-icon>
          </button>
          <button mat-icon-button color="warn">
            <mat-icon>delete</mat-icon>
          </button>
        </td>
      </ng-container>

      <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
      <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
    </table>

    <div *ngIf="templates().length === 0" class="p-4 text-center text-slate-500 bg-slate-50 rounded-lg mt-2">
      No templates defined.
    </div>
  `
})
export class FormTemplatesComponent implements OnChanges {
    @Input() formId!: string;
    private formsService = inject(FormsService);

    templates = signal<any[]>([]);
    displayedColumns = ['subject', 'enabled', 'actions'];

    ngOnChanges(changes: SimpleChanges) {
        if (changes['formId'] && this.formId) {
            this.loadTemplates();
        }
    }

    loadTemplates() {
        this.formsService.getTemplates(this.formId).subscribe({
            next: (data) => this.templates.set(data),
            error: (err) => console.error('Failed to load templates', err)
        });
    }

    toggle(tpl: any, checked: boolean) {
        const obs = checked
            ? this.formsService.enableTemplate(this.formId, tpl.id)
            : this.formsService.disableTemplate(this.formId, tpl.id);

        obs.subscribe({
            next: () => {
                // Update local state or reload
                tpl.isEnabled = checked;
            },
            error: (err) => {
                console.error('Failed to toggle template', err);
                // Revert UI
                tpl.isEnabled = !checked;
            }
        });
    }
}
