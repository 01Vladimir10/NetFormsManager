import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FormRequestDto, FormField, EmailTemplateRequestDto } from './models';

@Injectable({
    providedIn: 'root'
})
export class FormsService {
    private http = inject(HttpClient);

    getForms(): Observable<any> {
        return this.http.get('/forms');
    }

    createForm(form: FormRequestDto): Observable<any> {
        return this.http.post('/forms', form);
    }

    getForm(formId: string): Observable<any> {
        return this.http.get(`/forms/${formId}`);
    }

    updateForm(formId: string, form: FormRequestDto): Observable<any> {
        return this.http.put(`/forms/${formId}`, form);
    }

    deleteForm(formId: string): Observable<any> {
        return this.http.delete(`/forms/${formId}`);
    }

    getFormFields(formId: string): Observable<FormField[]> {
        return this.http.get<FormField[]>(`/forms/${formId}/fields`);
    }

    getSubscribers(formId: string, page = 1, pageSize = 100): Observable<any> {
        const params = new HttpParams().set('page', page).set('pageSize', pageSize);
        return this.http.get(`/forms/${formId}/subscribers`, { params });
    }

    exportSubscribers(formId: string, format = 'csv'): Observable<Blob> {
        const params = new HttpParams().set('format', format);
        return this.http.get(`/forms/${formId}/subscribers/export`, { params, responseType: 'blob' });
    }

    getTemplates(formId: string): Observable<any> {
        return this.http.get(`/forms/${formId}/templates`);
    }

    createTemplate(formId: string, template: EmailTemplateRequestDto): Observable<any> {
        return this.http.post(`/forms/${formId}/templates`, template);
    }

    getTemplate(formId: string, templateId: string): Observable<any> {
        return this.http.get(`/forms/${formId}/templates/${templateId}`);
    }

    updateTemplate(formId: string, templateId: string, template: EmailTemplateRequestDto): Observable<any> {
        return this.http.put(`/forms/${formId}/templates/${templateId}`, template);
    }

    deleteTemplate(formId: string, templateId: string): Observable<any> {
        return this.http.delete(`/forms/${formId}/templates/${templateId}`);
    }

    updateTemplateBody(formId: string, templateId: string, body: string): Observable<any> {
        return this.http.put(`/forms/${formId}/templates/${templateId}/body`, body); // Body might need to be wrapped or sent as raw string depending on backend, assuming raw string or object based on spec? Spec says PUT body is... wait, spec for body update?
        // /forms/{formId}/templates/{templateId}/body PUT. Request body?
        // Spec says: 
        // "/forms/{formId}/templates/{templateId}/body": { "put": { ... } }
        // But doesn't explicitly show requestBody schema in the snippet provided for that path? 
        // Ah, wait. The snippet provided is incomplete or I missed it.
        // Let's look at the spec again.
        // It says:
        // "/forms/{formId}/templates/{templateId}/body": { "put": { ... "responses": { "204": ... } } }
        // It doesn't show requestBody in the snippet I have. 
        // I will assume it takes the body content as a string or a simple object.
        // I'll leave it for now or assume raw string.
    }

    enableTemplate(formId: string, templateId: string): Observable<any> {
        return this.http.put(`/forms/${formId}/templates/${templateId}/enable`, {});
    }

    disableTemplate(formId: string, templateId: string): Observable<any> {
        return this.http.put(`/forms/${formId}/templates/${templateId}/disable`, {});
    }
}
