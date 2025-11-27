import { Component, inject, signal, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsService } from '../../core/api/forms.service';

// Placeholder imports for sub-components
import { FormFieldsComponent } from './form-fields.component';
import { FormTemplatesComponent } from './form-templates.component';
import { FormSubscribersComponent } from './form-subscribers.component';

@Component({
    selector: 'app-form-detail',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        MatTabsModule,
        MatButtonModule,
        MatIconModule,
        FormFieldsComponent,
        FormTemplatesComponent,
        FormSubscribersComponent
    ],
    template: `
    <div class="container mx-auto p-6">
      <div class="flex items-center gap-4 mb-6">
        <a mat-icon-button routerLink="/dashboard">
          <mat-icon>arrow_back</mat-icon>
        </a>
        <h1 class="text-3xl font-bold text-slate-900" *ngIf="form()">{{ form().name }}</h1>
      </div>

      <mat-tab-group animationDuration="0ms" class="bg-white rounded-lg shadow">
        <mat-tab label="Fields">
          <div class="p-6">
            <app-form-fields [formId]="formId" *ngIf="formId"></app-form-fields>
          </div>
        </mat-tab>
        <mat-tab label="Templates">
          <div class="p-6">
            <app-form-templates [formId]="formId" *ngIf="formId"></app-form-templates>
          </div>
        </mat-tab>
        <mat-tab label="Subscribers">
          <div class="p-6">
            <app-form-subscribers [formId]="formId" *ngIf="formId"></app-form-subscribers>
          </div>
        </mat-tab>
      </mat-tab-group>
    </div>
  `
})
export class FormDetailComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private formsService = inject(FormsService);

    formId: string = '';
    form = signal<any>(null);

    ngOnInit() {
        this.route.paramMap.subscribe(params => {
            this.formId = params.get('id') || '';
            if (this.formId) {
                this.loadForm();
            }
        });
    }

    loadForm() {
        this.formsService.getForm(this.formId).subscribe({
            next: (data) => this.form.set(data),
            error: (err) => console.error('Failed to load form', err)
        });
    }
}
