import { Injectable, signal, computed, effect } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly TOKEN_KEY = 'nfm_token';
    private readonly URL_KEY = 'nfm_url';
    private readonly REMEMBER_KEY = 'nfm_remember';

    // Signals
    private _token = signal<string | null>(null);
    private _baseUrl = signal<string>('http://localhost:8080'); // Default
    private _rememberMe = signal<boolean>(false);

    token = computed(() => this._token());
    baseUrl = computed(() => this._baseUrl());
    isAuthenticated = computed(() => !!this._token());

    constructor() {
        this.loadFromStorage();
    }

    private loadFromStorage() {
        // Check local storage first (Remember Me)
        const localToken = localStorage.getItem(this.TOKEN_KEY);
        const localUrl = localStorage.getItem(this.URL_KEY);

        if (localToken) {
            this._token.set(localToken);
            this._rememberMe.set(true);
            if (localUrl) this._baseUrl.set(localUrl);
            return;
        }

        // Check session storage
        const sessionToken = sessionStorage.getItem(this.TOKEN_KEY);
        const sessionUrl = sessionStorage.getItem(this.URL_KEY);

        if (sessionToken) {
            this._token.set(sessionToken);
            this._rememberMe.set(false);
            if (sessionUrl) this._baseUrl.set(sessionUrl);
        }
    }

    connect(token: string, url: string, remember: boolean) {
        this._token.set(token);
        this._baseUrl.set(url);
        this._rememberMe.set(remember);

        const storage = remember ? localStorage : sessionStorage;

        // Clear other storage
        const other = remember ? sessionStorage : localStorage;
        other.removeItem(this.TOKEN_KEY);
        other.removeItem(this.URL_KEY);

        storage.setItem(this.TOKEN_KEY, token);
        storage.setItem(this.URL_KEY, url);
    }

    disconnect() {
        this._token.set(null);
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.URL_KEY);
        sessionStorage.removeItem(this.TOKEN_KEY);
        sessionStorage.removeItem(this.URL_KEY);
    }
}
