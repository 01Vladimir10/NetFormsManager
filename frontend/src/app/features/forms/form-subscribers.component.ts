import { Component, inject, signal, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsService } from '../../core/api/forms.service';

@Component({
    selector: 'app-form-subscribers',
    standalone: true,
    imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule],
    template: `
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-xl font-semibold text-slate-800">Subscribers</h2>
      <button mat-stroked-button color="primary" (click)="export()">
        <mat-icon>download</mat-icon>
        Export CSV
      </button>
    </div>

    <div class="overflow-x-auto border border-slate-200 rounded-lg">
      <table mat-table [dataSource]="subscribers()" class="w-full">
        <!-- Dynamic columns based on data would be ideal, but for now assuming some standard fields or just displaying raw data -->
        <!-- Since we don't know exact subscriber fields structure (it's dynamic), we might need to just show JSON or specific known fields if any. 
             The spec doesn't strictly define subscriber output schema other than it's a list. 
             Let's assume it's a key-value pair object. -->
        
        <ng-container matColumnDef="data">
          <th mat-header-cell *matHeaderCellDef> Data </th>
          <td mat-cell *matCellDef="let sub"> 
            <pre class="text-xs">{{ sub | json }}</pre> 
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="['data']"></tr>
        <tr mat-row *matRowDef="let row; columns: ['data'];"></tr>
      </table>
    </div>

    <div *ngIf="subscribers().length === 0" class="p-4 text-center text-slate-500 bg-slate-50 rounded-lg mt-2">
      No subscribers found.
    </div>
  `
})
export class FormSubscribersComponent implements OnChanges {
    @Input() formId!: string;
    private formsService = inject(FormsService);

    subscribers = signal<any[]>([]);

    ngOnChanges(changes: SimpleChanges) {
        if (changes['formId'] && this.formId) {
            this.loadSubscribers();
        }
    }

    loadSubscribers() {
        this.formsService.getSubscribers(this.formId).subscribe({
            next: (data) => {
                // API returns object with items? or array? Spec says "items": { ... } type: array.
                // But usually paged response has { items: [], total: ... } or just [].
                // Spec: "/forms/{formId}/subscribers": { "get": { "responses": { "200": { "description": "OK" } } } }
                // It doesn't explicitly show the schema in the snippet provided for the response.
                // Assuming array for now based on typical usage, or I'll inspect it.
                if (Array.isArray(data)) {
                    this.subscribers.set(data);
                } else if (data && Array.isArray(data.items)) {
                    this.subscribers.set(data.items);
                } else {
                    this.subscribers.set([]);
                }
            },
            error: (err) => console.error('Failed to load subscribers', err)
        });
    }

    export() {
        this.formsService.exportSubscribers(this.formId).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `subscribers-${this.formId}.csv`;
                a.click();
                window.URL.revokeObjectURL(url);
            },
            error: (err) => console.error('Failed to export', err)
        });
    }
}
