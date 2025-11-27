import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BotValidationProviderDto } from './models';

@Injectable({
    providedIn: 'root'
})
export class ProvidersService {
    private http = inject(HttpClient);

    getBotDetectors(): Observable<BotValidationProviderDto[]> {
        return this.http.get<BotValidationProviderDto[]>('/providers/botDetectors');
    }

    getBotDetector(name: string): Observable<any> {
        return this.http.get(`/providers/botDetectors/${name}`);
    }

    getSubscriptions(): Observable<any> {
        return this.http.get('/providers/subscriptions');
    }
}
